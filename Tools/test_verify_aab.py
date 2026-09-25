import tempfile
import unittest
from pathlib import Path
from zipfile import ZipFile
from verify_aab import verify


class BundleVerificationTests(unittest.TestCase):
    def test_required_entries_and_checksum(self):
        with tempfile.TemporaryDirectory() as directory:
            path = Path(directory) / 'test.aab'
            with ZipFile(path, 'w') as archive:
                archive.writestr('BundleConfig.pb', b'config')
                archive.writestr('base/manifest/AndroidManifest.xml', b'manifest')
            self.assertEqual(64, len(verify(path)))

    def test_missing_or_empty_manifest_rejected(self):
        for manifest in (None, b''):
            with self.subTest(manifest=manifest), tempfile.TemporaryDirectory() as directory:
                path = Path(directory) / 'bad.aab'
                with ZipFile(path, 'w') as archive:
                    archive.writestr('BundleConfig.pb', b'config')
                    if manifest is not None:
                        archive.writestr('base/manifest/AndroidManifest.xml', manifest)
                with self.assertRaises(ValueError):
                    verify(path)


if __name__ == '__main__':
    unittest.main()
