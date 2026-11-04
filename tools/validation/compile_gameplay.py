#!/usr/bin/env python3
"""Compile project C# outside Unity using its existing Bee response files (no Editor launch).
Run from the repository root after Unity has compiled this project at least once.
Outputs stay in a temporary directory; no Library or asset changes are made.
"""
import pathlib
import re
import subprocess
import tempfile

ROOT = pathlib.Path(__file__).resolve().parents[2]
EDITOR = pathlib.Path('/Applications/Unity/Hub/Editor/6000.6.0f1/Unity.app/Contents')
SDK = EDITOR / 'Resources/Scripting/DotNetSdk'
CSC = next((SDK / 'sdk').glob('*/Roslyn/bincore/csc.dll'))
OUT = pathlib.Path(tempfile.mkdtemp(prefix='shrink-gameplay-check-'))


def compile_assembly(name, extra_sources, replacements=None):
    candidates = list((ROOT / 'Library/Bee/artifacts').glob('*/' + name + '.rsp'))
    if not candidates:
        raise SystemExit('No Unity Bee response file for ' + name + '; compile in Unity first.')
    source = max(candidates, key=lambda p: p.stat().st_mtime)
    lines = source.read_text().splitlines()
    result = []
    seen = set()
    for line in lines:
        if line.startswith(('-out:', '-refout:', '-analyzer:', '/additionalfile:')):
            continue
        if line.startswith('"') and line.endswith('.cs"'):
            seen.add(line.strip('"'))
        if replacements:
            for old_name, new_path in replacements.items():
                line = re.sub(r'(?<=\")([^\"]*/)?' + re.escape(old_name) + r'(?=\")', str(new_path), line)
        result.append(line)
    result.append('-out:"' + str(OUT / (name + '.dll')) + '"')
    for path in extra_sources:
        relative = path.relative_to(ROOT).as_posix()
        if relative not in seen:
            result.append('"' + relative + '"')
    response = OUT / (name + '.rsp')
    response.write_text('\n'.join(result) + '\n')
    subprocess.run([str(SDK / 'dotnet'), str(CSC), '@' + str(response)], cwd=ROOT, check=True)
    print(name + ': C# compilation passed', flush=True)


compile_assembly('Assembly-CSharp', (ROOT / 'Assets/_Game/Core/Scripts').rglob('*.cs'))
compile_assembly('Assembly-CSharp-Editor', (ROOT / 'Assets/Editor').rglob('*.cs'), {
    'Assembly-CSharp.dll': OUT / 'Assembly-CSharp.dll',
    'Assembly-CSharp.ref.dll': OUT / 'Assembly-CSharp.dll',
})
print('External compiler artifacts: ' + str(OUT))
print('This does not verify Unity import, serialized assets, playmode, or Android builds.')
