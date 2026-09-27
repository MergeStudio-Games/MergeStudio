# Device verification and release

## Evidence, not assumptions

Run `Tools/Test-Workspace.ps1 -BuildAndroid` for local tests and an AAB. Record
device metadata with `Tools/Get-AndroidDeviceReport.ps1`; this does not install
software, clear application data, or mark any gameplay check as passed.

An AAB is a distribution artifact, not an APK that adb can install directly.
Use the Play internal testing track or an approved bundletool installation to
obtain device-specific APKs. Do not treat archive integrity as a signing check.

## Device acceptance record

Copy this table into a per-build QA record under `artifacts/`. Every result begins
as NOT RUN. Record commit, artifact SHA256, tester and UTC date before testing.

| Scenario | Evidence to record | Result |
| --- | --- | --- |
| Cold start | Startup duration and screen recording | NOT RUN |
| First session | Generate, merge, complete order, reward once | NOT RUN |
| Touch | Tap and drag; invalid target preserves both items | NOT RUN |
| Persistence | Background/force-close/relaunch retains progress | NOT RUN |
| Recovery | Corrupt-save fixture recovers backup without erasing data | NOT RUN |
| Offline | Startup and local gameplay with airplane mode | NOT RUN |
| Layout | Small/tall/notched screen, readable text, safe area | NOT RUN |
| Localization | TR/EN and unsupported system language | NOT RUN |
| Performance | 10-minute session, frame times, memory, thermal behavior | NOT RUN |
| Accessibility | Contrast, text size, non-color feedback, mute | NOT RUN |

Set budgets from measurements on a named low-end device. The initial design
target is 60 FPS (16.7 ms frame budget), not a claim of current performance.

## Release gate

1. Select a reviewed commit; keep tests and build evidence from that same revision.
2. Verify unique application ID, increasing Android version code and release notes.
3. Use an upload keystore backed up outside Git. Never generate a disposable key
   and call it a production release key.
4. Configure signing in the game's CI environment using its secret store.
5. Verify the signed artifact on Play internal testing and complete the device record.
6. Confirm per-game privacy/support URLs, content ratings, assets and licenses.
7. Archive AAB, `.aab.json` manifest, test XML, device record and release notes.
8. Keep the previous known-good artifact. A Play rollback normally requires a new
   higher version code; retaining an old binary is not an automatic store rollback.

No store submission or signing setup has been performed by these tools.
