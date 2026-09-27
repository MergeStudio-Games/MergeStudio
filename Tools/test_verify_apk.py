from pathlib import Path
from tempfile import TemporaryDirectory
from zipfile import ZipFile

from verify_apk import verify


def test_verify_apk_accepts_required_entries():
    with TemporaryDirectory() as folder:
        apk = Path(folder) / 'game.apk'
        with ZipFile(apk, 'w') as archive:
            archive.writestr('AndroidManifest.xml', b'manifest')
            archive.writestr('classes.dex', b'dex')
            archive.writestr('lib/arm64-v8a/libunity.so', b'unity')
        assert len(verify(apk)) == 64
