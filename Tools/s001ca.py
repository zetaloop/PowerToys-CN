n=[]
while True:
    x=input('>>> ')
    if not x:break
    x=x.strip()
    n.append(x)
print('\n\n'+'='*16)
print(repr(n))
import pyperclip
pyperclip.copy(repr(n))
print('\n(copied to clipboard)')
print('\nPRESS ANY KEY TO CONTINUE...')
input()