"""Require fresh-in-this-job, passing EditMode and PlayMode NUnit reports."""
import argparse
from pathlib import Path
import xml.etree.ElementTree as ET


def verify(directory):
    modes = set()
    total = 0
    for path in Path(directory).rglob('*.xml'):
        root = ET.parse(path).getroot()
        if root.tag != 'test-run':
            continue
        name = path.name.lower()
        mode = next((mode for mode in ('editmode', 'playmode') if mode in name), None)
        if mode is None:
            continue
        if (root.get('result') != 'Passed' or int(root.get('total', '0')) <= 0
                or int(root.get('failed', '-1')) != 0 or int(root.get('skipped', '-1')) != 0
                or int(root.get('passed', '0')) != int(root.get('total', '0'))):
            raise ValueError(f'Incomplete or failing Unity suite: {path}')
        modes.add(mode)
        total += int(root.get('passed'))
    if modes != {'editmode', 'playmode'}:
        raise ValueError('Both EditMode and PlayMode passing reports are required')
    return total


if __name__ == '__main__':
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument('directory', type=Path)
    args = parser.parse_args()
    print(f'Unity XML verified: {verify(args.directory)} passed, zero failed/skipped')
