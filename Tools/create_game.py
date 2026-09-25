"""Create a local game from committed template files, with independent Git history."""
import argparse
import hashlib
import json
from pathlib import Path
import re
import subprocess
import tempfile
from zipfile import ZipFile


def git(source, *args):
    return subprocess.check_output(['git', '-C', str(source), *args], text=True).strip()


def create(source, destination, product, company, identifier, revision='develop'):
    source, destination = Path(source).resolve(), Path(destination).resolve()
    if destination.exists():
        raise ValueError('Destination already exists; existing files will not be replaced')
    if not re.fullmatch(r'[a-z][a-z0-9]*(?:\.[a-z][a-z0-9]*){2,}', identifier):
        raise ValueError('Application ID must have at least three lowercase alphanumeric segments')
    for label, value in [('Product', product), ('Company', company)]:
        if not value.strip() or any(ord(c) < 32 for c in value):
            raise ValueError(f'{label} must be nonempty text without control characters')
    commit = git(source, 'rev-parse', '--verify', revision + '^{commit}')
    # Archive only committed files, never local credentials or the template history.
    with tempfile.TemporaryDirectory(prefix='mergestudio-template-') as temporary:
        archive = Path(temporary) / 'template.zip'
        git(source, 'archive', '--format=zip', '--output=' + str(archive), commit)
        with ZipFile(archive) as bundle:
            bundle.extractall(destination)
    # Archive contains LFS pointers, not their content. Materialize before the
    # independent repository is initialized so it can upload its own LFS objects.
    for path in destination.rglob('*'):
        if not path.is_file() or path.stat().st_size > 2048:
            continue
        pointer = path.read_bytes()
        if not pointer.startswith(b'version https://git-lfs.github.com/spec/v1\n'):
            continue
        content = subprocess.check_output(['git', '-C', str(source), 'lfs', 'smudge', str(path.relative_to(destination))], input=pointer)
        expected = re.search(rb'oid sha256:([a-f0-9]{64})', pointer)
        if expected is None or hashlib.sha256(content).hexdigest().encode() != expected[1]:
            raise ValueError(f'Could not materialize LFS content: {path.relative_to(destination)}')
        path.write_bytes(content)
    settings = destination / 'ProjectSettings/ProjectSettings.asset'
    text = settings.read_text(encoding='utf-8')
    for key, value in [('companyName', company), ('productName', product)]:
        text, count = re.subn(r'^  ' + key + r':.*$', lambda m: '  ' + key + ': ' + json.dumps(value, ensure_ascii=False), text, flags=re.M)
        if count != 1:
            raise ValueError(f'Expected exactly one {key} in ProjectSettings')
    text, count = re.subn(r'(  applicationIdentifier:\n)(?:(?:    .*\n)+)',
                         '  applicationIdentifier:\n    Android: ' + identifier + '\n    iPhone: ' + identifier + '\n', text)
    if count != 1:
        raise ValueError('Application identifier block missing')
    settings.write_text(text, encoding='utf-8')
    docs = destination / 'Docs'
    docs.mkdir(exist_ok=True)
    (docs / 'GAME-IDENTITY.json').write_text(json.dumps({
        'product': product, 'company': company, 'applicationId': identifier,
        'templateCommit': commit, 'status': 'prototype',
    }, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
    git(destination, 'init', '-b', 'develop')
    return commit


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('--source', type=Path, default=Path(__file__).resolve().parents[1])
    parser.add_argument('--destination', type=Path, required=True)
    parser.add_argument('--product', required=True)
    parser.add_argument('--company', default='MergeStudio Games')
    parser.add_argument('--identifier', required=True)
    parser.add_argument('--revision', default='develop')
    args = parser.parse_args()
    commit = create(args.source, args.destination, args.product, args.company, args.identifier, args.revision)
    print(f'Created {args.destination} from {commit}; independent Git repository on develop.')
