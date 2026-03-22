with open(r'E:\LevelDesign\Steal\Assets\Documents\Protagonist_MeshyPrompt_INTI_FALL.md', encoding='utf-8') as f:
    lines = f.readlines()

in_block = False
start = end = 0
for i, line in enumerate(lines):
    stripped = line.strip()
    if stripped == '```':
        if not in_block:
            in_block = True
            start = i + 1
        else:
            end = i
            break

prompt_lines = lines[start:end]
total = sum(len(l.rstrip()) for l in prompt_lines)
print(f'Lines: {end - start}')
print(f'Total chars (no trailing ws): {total}')

# Show each line and its length
for i, l in enumerate(prompt_lines):
    print(f'{i}: [{len(l.rstrip())}] {l.rstrip()}')