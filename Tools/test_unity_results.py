import tempfile
import unittest
from pathlib import Path
from verify_unity_results import verify


class UnityEvidenceTests(unittest.TestCase):
    def test_both_suites_required(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            report = '<test-run result="Passed" total="2" passed="2" failed="0" skipped="0"/>'
            (root / 'EditMode.xml').write_text(report)
            with self.assertRaises(ValueError):
                verify(root)
            (root / 'playmode-results.xml').write_text(report)
            self.assertEqual(4, verify(root))

    def test_empty_failed_skipped_and_inconsistent_reports_rejected(self):
        for attributes in ('total="0" passed="0" failed="0" skipped="0"',
                           'total="2" passed="1" failed="1" skipped="0"',
                           'total="2" passed="1" failed="0" skipped="1"',
                           'total="2" passed="1" failed="0" skipped="0"'):
            with self.subTest(attributes=attributes), tempfile.TemporaryDirectory() as directory:
                for mode in ('EditMode', 'PlayMode'):
                    (Path(directory) / (mode + '.xml')).write_text(f'<test-run result="Passed" {attributes}/>')
                with self.assertRaises(ValueError):
                    verify(directory)


if __name__ == '__main__':
    unittest.main()
