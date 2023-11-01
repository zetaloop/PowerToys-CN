import os
import pinyin

# 设置翻译文件所在的目录路径以及一些必要的初始化变量
translations_directory = os.path.abspath(os.path.join(os.getcwd(), '..')).replace('\\','/')  # 设置为当前目录的上一级目录
initial_languages = ['All', 'Currency', 'Pinyin']
os.chdir(translations_directory)

def get_power_toys_versions():
    """获取指定目录下所有的 PowerToys 版本"""
    versions = sorted(i for i in os.listdir() if i.startswith('PowerToys-') and i[-1].isdigit())
    print(f'发现版本：{", ".join(versions)}')
    return versions  # 返回所有版本

def get_latest_version(versions):
    """获取最新的 PowerToys 版本"""
    try:
        latest_version = versions[-1]  # 尝试获取最新的版本
        print(f'选定版本：{latest_version}')
        return latest_version  # 返回最新版本
    except IndexError:
        print('无可用版本！')
        input('[按任意键退出...]')
        exit()  # 如果没有可用版本，退出程序

def get_translations_and_replacements(latest_version):
    """获取给定版本的翻译数据和需要替换的数据"""
    os.chdir(latest_version)
    os.chdir('src/settings-ui/Settings.UI')

    original_texts = []  # 保存原始文本
    translations = []  # 保存翻译文本
    replacements_in_view = []  # 保存需要在视图中替换的内容
    replacements_in_model = []  # 保存需要在模型中替换的内容
    read_next_line = False  # 用于标识是否读取下一行
    with open('Strings/en-us/Resources.resw', encoding='utf-8') as f:
        lines = f.readlines()

    for line in lines:
        if read_next_line:
            translations.append(line.split('<value>')[-1].split('</value>')[0])  # 读取翻译
            read_next_line = False
        if 'QuickAccent_SelectedLanguage_' in line:
            original_texts.append(line.split('SelectedLanguage_')[-1].split('.Content')[0])  # 读取原始文本
            read_next_line = True

    with open('Views/PowerAccentPage.xaml', encoding='utf-8') as f:
        lines = f.readlines()
        for line in lines:
            if 'QuickAccent_SelectedLanguage_' in line:
                replacements_in_view.append(line)  # 保存需要在视图中替换的内容

    with open('ViewModels/PowerAccentViewModel.cs', encoding='utf-8') as f:
        lines = f.readlines()
        for line in lines:
            if len(line.strip()) < 7 and line.upper() == line:
                replacements_in_model.append(line)  # 保存需要在模型中替换的内容

    return dict(zip(original_texts, translations)), replacements_in_view, replacements_in_model  # 返回原始文本与翻译的映射，以及需要替换的内容

def sort_translations(translations):
    """将翻译按照拼音排序"""
    sorted_translations = sorted(
        (pinyin.get(translated_text, format='strip'), original_text)
        for original_text, translated_text in translations.items()
    )
    lst = ['All', 'Currency', 'Pinyin']
    lst += [original_text for _, original_text in sorted_translations if original_text not in lst]
    return lst  # 返回排序后的翻译

def replace_translations_in_file(file_path, original_texts, sorted_translations):
    """在给定文件中替换原始文本为翻译后的文本"""
    with open(file_path, encoding='utf-8') as f:
        content = f.read()  # 读取文件内容
    temp_replacements = [f'00{i}'[-3:]+'XXXXAUTOTRANSLATIONXXXX' for i in range(len(sorted_translations))]  # 生成临时替换文本
    for original, temp in zip(original_texts, temp_replacements):
        content = content.replace(original, temp)  # 将原始文本替换为临时替换文本
    for temp, sorted_text in zip(temp_replacements, sorted_translations):
        content = content.replace(temp, sorted_text)  # 将临时替换文本替换为排序后的翻译
    with open(file_path, 'w', encoding='utf-8') as f:
        f.write(content)  # 写回文件

def main():
    """主函数，完成全部工作流程"""
    versions = get_power_toys_versions()  # 获取所有版本
    latest_version = get_latest_version(versions)  # 获取最新版本
    translations, replacements_in_view, replacements_in_model = get_translations_and_replacements(latest_version)  # 获取翻译和需要替换的内容
    sorted_translations = sort_translations(translations)  # 将翻译排序
    for i in sorted_translations:
        print(f'\n{i} >>> {translations[i]}')  # 打印排序后的翻译
    replace_translations_in_file('Views/PowerAccentPage.xaml', replacements_in_view, sorted_translations)  # 在文件中替换翻译
    replace_translations_in_file('ViewModels/PowerAccentViewModel.cs', replacements_in_model, sorted_translations)  # 在文件中替换翻译
    print('OK.\n')
    input('[按任意键退出...]')  # 等待用户操作后退出

if __name__ == "__main__":
    main()  # 如果是作为脚本运行，调用 main 函数
