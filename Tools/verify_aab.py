"""Validate bundle structure and CRC; this does not certify signing/device behavior."""
import argparse
import hashlib
from pathlib import Path
from zipfile import ZipFile


def verify(path):
    with ZipFile(path) as archive:
        required = {'BundleConfig.pb', 'base/manifest/AndroidManifest.xml'}
        if not required <= set(archive.namelist()):
            raise ValueError('Missing required Android bundle entries')
        for name in required:
            if not archive.read(name):
                raise ValueError(f'Empty bundle entry: {name}')
        corrupt = archive.testzip()
        if corrupt:
            raise ValueError(f'Corrupt bundle entry: {corrupt}')
    with path.open('rb') as stream:
        digest = hashlib.sha256()
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(chunk)
    return digest.hexdigest()


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('bundle', type=Path)
    args = parser.parse_args()
    print(f'Verified {args.bundle}: SHA256 {verify(args.bundle)}')
