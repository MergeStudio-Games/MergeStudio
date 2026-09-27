import subprocess
import shutil
import tempfile
import unittest
from pathlib import Path
from create_game import create, git


class GameCreationTests(unittest.TestCase):
    @unittest.skipUnless(shutil.which('git-lfs'), 'Git LFS not installed')
    def test_lfs_content_materialized_in_new_project(self):
        with tempfile.TemporaryDirectory() as directory:
            source = Path(directory) / 'source'; source.mkdir()
            git(source, 'init', '-b', 'develop'); git(source, 'lfs', 'install', '--local')
            (source / '.gitattributes').write_text('*.bin filter=lfs diff=lfs merge=lfs -text\n')
            content = b'game asset\x00' * 300
            (source / 'art.bin').write_bytes(content)
            (source / 'ProjectSettings').mkdir()
            (source / 'ProjectSettings/ProjectSettings.asset').write_text('PlayerSettings:\n  companyName: Template\n  productName: Template\n  applicationIdentifier:\n    Android: com.old.game\n')
            git(source, 'add', '.')
            git(source, '-c', 'user.name=Fixture', '-c', 'user.email=fixture@example.invalid', 'commit', '-m', 'LFS fixture')
            target = Path(directory) / 'game'
            create(source, target, 'Game', 'Studio', 'com.studio.game')
            self.assertEqual(content, (target / 'art.bin').read_bytes())

    def test_independent_history_and_identity(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            source = root / 'source'
            source.mkdir()
            git(source, 'init', '-b', 'develop')
            settings = source / 'ProjectSettings'
            settings.mkdir()
            (settings / 'ProjectSettings.asset').write_text('PlayerSettings:\n  companyName: Template\n  productName: Template\n  applicationIdentifier:\n    Android: com.old.game\n    iPhone: com.old.game\n  buildNumber:\n    iPhone: 0\n')
            git(source, 'add', '.')
            git(source, '-c', 'user.name=Fixture', '-c', 'user.email=fixture@example.invalid', 'commit', '-m', 'fixture')
            (source / 'untracked-secret.txt').write_text('must not be copied')
            target = root / 'game'
            create(source, target, 'Bread: Shop', 'Studio', 'com.studio.bread')
            text = (target / 'ProjectSettings/ProjectSettings.asset').read_text()
            self.assertIn('productName: "Bread: Shop"', text)
            self.assertIn('Android: com.studio.bread', text)
            self.assertIn('  buildNumber:\n    iPhone: 0', text)
            self.assertFalse((target / 'untracked-secret.txt').exists())
            self.assertEqual('', git(target, 'remote'))
            result = subprocess.run(['git', '-C', str(target), 'rev-parse', '--verify', 'HEAD'], capture_output=True)
            self.assertNotEqual(0, result.returncode)
            with self.assertRaises(ValueError):
                create(source, target, 'Other', 'Studio', 'com.studio.other')

    def test_rejects_bad_identity_before_creating_directory(self):
        with tempfile.TemporaryDirectory() as directory:
            target = Path(directory) / 'game'
            with self.assertRaises(ValueError):
                create(directory, target, 'Game', 'Studio', 'bad id')
            self.assertFalse(target.exists())


if __name__ == '__main__':
    unittest.main()
