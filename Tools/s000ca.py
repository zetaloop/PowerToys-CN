n=''
while True:
    x=input('>>> ')
    if not x:break
    x=x.strip()
    if x.startswith('<value>') or x.startswith('<ComboBoxItem'):
        x=x.replace('à', 'a')
        x=x.replace('All available', 'All')
        x=x.replace('<value>', '')
        x=x.replace('</value>', '')
        if 'ComboBoxItem' in x:
            x=x.split('"')[-2].split('_')[-1]
        x=x.strip()
        n+=x+'\n'
print('\n\n'+'='*16)
print(n)
import pyperclip
pyperclip.copy(n)
print('\n(copied to clipboard)')
print('\nPRESS ANY KEY TO CONTINUE...')
input()