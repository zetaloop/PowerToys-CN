table=[[]]
prompt=3

while True:
    c=input('>'*prompt+' ')
    if c:
        prompt=3
        table[-1].append(c)
    else:
        prompt-=1
        if prompt==2:
            table.append([])
        if prompt==0:
            break

print('\n'+'='*16)

table.reverse()
result=[[i] for i in table[-1]]
while len(table)>1:
    dic=dict(zip(table.pop(), table.pop()))
    result=[[dic[n[0]]] + n for n in result if n[0] in dic]

import pinyin
for i in result:
    i.insert(0, pinyin.get(i[0]))

result.sort()
final='\n'.join([n[-1] for n in result])
print(final)

import pyperclip
pyperclip.copy(final)
print('\n(copied to clipboard)')
print('\nPRESS ANY KEY TO CONTINUE...')
input()