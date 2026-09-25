"""Record artifact provenance; no signing or store-readiness claim is made."""
import argparse
from datetime import datetime, timezone
import json
from pathlib import Path
import subprocess
from verify_aab import verify


def manifest(bundle, output):
    root = Path(__file__).resolve().parents[1]
    digest = verify(bundle)
    commit = subprocess.check_output(['git', '-C', str(root), 'rev-parse', 'HEAD'], text=True).strip()
    dirty = bool(subprocess.check_output(['git', '-C', str(root), 'status', '--porcelain'], text=True).strip())
    data = {'utc': datetime.now(timezone.utc).isoformat(), 'commit': commit,
            'workingTreeDirty': dirty, 'bundle': bundle.name, 'bytes': bundle.stat().st_size,
            'sha256': digest, 'signingVerified': False, 'deviceQaVerified': False}
    with output.open('x', encoding='utf-8') as stream:
        json.dump(data, stream, indent=2)
        stream.write('\n')


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('bundle', type=Path)
    parser.add_argument('output', type=Path)
    args = parser.parse_args()
    manifest(args.bundle, args.output)
