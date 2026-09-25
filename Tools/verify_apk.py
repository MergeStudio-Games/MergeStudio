"""Validate APK structure and CRC; this does not certify signing/device behavior."""
import argparse
import hashlib
from pathlib import Path
from zipfile import ZipFile


def verify(path):
    with ZipFile(path) as archive:
        names = set(archive.namelist())
        required = {'AndroidManifest.xml', 'classes.dex', 'lib/arm64-v8a/libunity.so'}
        if not required <= names:
            raise ValueError('Missing required Android APK entries')
        for name in required:
            if not archive.read(name):
                raise ValueError(f'Empty APK entry: {name}')
        corrupt = archive.testzip()
        if corrupt:
            raise ValueError(f'Corrupt APK entry: {corrupt}')
    with path.open('rb') as stream:
        digest = hashlib.sha256()
        for chunk in iter(lambda: stream.read(1024 * 1024), b''):
            digest.update(chunk)
    return digest.hexdigest()


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('apk', type=Path)
    args = parser.parse_args()
    print(f'Verified {args.apk}: SHA256 {verify(args.apk)}')
