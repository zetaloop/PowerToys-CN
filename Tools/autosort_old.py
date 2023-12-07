import os
os.chdir('E:/Translations')
vers = sorted(i for i in os.listdir() if i.startswith('PowerToys-') and i[-1].isdigit())
print(f'发现版本：{", ".join(vers)}')
try:
    pt = vers[-1]
    # like 'PowerToys-0.67.0'
    print(f'选定版本：{pt}')
except:
    print('无可用版本！')
    input('[按任意键退出...]')
print('\n确保 Resources.resw 是已翻译的，'
'将会自动排序修改 PowerAccentPage.xaml 和 PowerAccentViewModel.cs。\n')
input('[按任意键开始]')

orig = []
trans = []
readnextline = False
os.chdir(pt)
os.chdir('src/settings-ui/Settings.UI')

with open('Strings/en-us/Resources.resw', encoding='utf-8') as f:
    resw = f.readlines()
for line in resw:
    if readnextline:
        trans.append(line.split('<value>')[-1].split('</value>')[0])
        readnextline = False
    if 'QuickAccent_SelectedLanguage_' in line:
        orig.append(line.split('SelectedLanguage_')[-1].split('.Content')[0])
        readnextline = True

with open('Views/PowerAccentPage.xaml', encoding='utf-8') as f:
    o1 = [l for l in f.readlines() if 'QuickAccent_SelectedLanguage_' in l]
sort = [l.split('SelectedLanguage_')[-1].split('"')[0] for l in o1]
with open('ViewModels/PowerAccentViewModel.cs', encoding='utf-8') as f:
    o2 = [l for l in f.readlines() if len(l.strip())<7 and l.upper()==l]

lst = ['All', 'Currency', 'Pinyin']
so1 = dict(zip(sort, o1))
so2 = dict(zip(sort, o2))
st = dict(zip(orig, trans))
import pinyin
x = sorted(zip((pinyin.get(i,format='strip') for i in trans), orig))
lst += [b for a,b in x if b not in lst]
s1 = [so1[i] for i in lst]
s2 = [so2[i] for i in lst]
ran1 = [f'00{i}'[-3:]+'XXXXAUTOTRANSLATIONXXXX' for i in range(len(s1))]
ran2 = [f'00{i}'[-3:]+'XXXXAUTOTRANSLATIONXXXX' for i in range(len(s2))]

for i in lst:
    print(f'\n{i} >>> {st[i]}\n{so1[i].strip()}\n{so2[i].strip()}')

with open('Views/PowerAccentPage.xaml', encoding='utf-8') as f:
    x1 = f.read()
    for a,b in zip(o1, ran1):
        x1 = x1.replace(a, b)
    for a,b in zip(ran1, s1):
        x1 = x1.replace(a, b)
with open('Views/PowerAccentPage.xaml', mode='w', encoding='utf-8') as f:
    print('\nWriting "Views/PowerAccentPage.xaml"')
    f.write(x1)
with open('ViewModels/PowerAccentViewModel.cs', encoding='utf-8') as f:
    x2 = f.read()
    for a,b in zip(o2, ran2):
        x2 = x2.replace(a, b)
    for a,b in zip(ran2, s2):
        x2 = x2.replace(a, b)
with open('ViewModels/PowerAccentViewModel.cs', mode='w', encoding='utf-8') as f:
    print('Writing "ViewModels/PowerAccentViewModel.cs"')
    f.write(x2)
print('OK.\n')
input('[按任意键退出...]')