import os
import pinyin

# 设置翻译文件所在的目录路径以及一些必要的初始化变量
translations_directory = os.path.dirname(__file__)  # os.getcwd()
translations_directory = os.path.abspath(os.path.join(translations_directory, "..\\.."))
translations_directory = translations_directory.replace("\\", "/")  # 设置为当前目录的上一级目录
print(translations_directory)
initial_languages = ["All", "Currency", "Pinyin"]
os.chdir(translations_directory)


def main():
    """主函数，完成全部工作流程"""
    versions = get_power_toys_versions()  # 获取所有版本
    latest_version = get_latest_version(versions)  # 获取最新版本
    lab2cn, lab2view, lab2model = initall(latest_version)  # 获取翻译和需要替换的内容
    sortedlab2cn = sort_label(lab2cn)  # 将翻译排序
    print("\n[排序完成]")
    for i in lab2cn:
        print(f"{i} >>> {lab2cn[i]}")  # 打印排序后的翻译
    input("\n[按任意键立即写入]")
    replace_translations_in_file(
        "SettingsXAML/Views/PowerAccentPage.xaml", lab2cn, sortedlab2cn, lab2view
    )  # 在文件中替换翻译
    replace_translations_in_file(
        "ViewModels/PowerAccentViewModel.cs", lab2cn, sortedlab2cn, lab2model
    )  # 在文件中替换翻译
    print("OK.\n")
    input("[按任意键退出...]")  # 等待用户操作后退出


def get_power_toys_versions():
    """获取指定目录下所有的 PowerToys 版本"""
    versions = sorted(
        i for i in os.listdir() if i.startswith("PowerToys-") and i[-1].isdigit()
    )
    print(f'发现版本：{", ".join(versions)}')
    return versions  # 返回所有版本


def get_latest_version(versions):
    """获取最新的 PowerToys 版本"""
    try:
        latest_version = versions[-1]  # 尝试获取最新的版本
        print(f"选定版本：{latest_version}\n")
        return latest_version  # 返回最新版本
    except IndexError:
        print("无可用版本！")
        input("[按任意键退出...]")
        exit()  # 如果没有可用版本，退出程序


def initall(latest_version):
    """获取给定版本的翻译数据和需要替换的数据"""
    os.chdir(latest_version)
    os.chdir("src/settings-ui/Settings.UI")

    LABELS = []  # 标签，RESC顺序
    NAMESCN = []  # 中文，RESC顺序
    VLABLES = []  # 标签，View顺序
    VIEWS_ln = []  # View行
    MODELS_ln = []  # ViewModel行
    with open("Strings/en-us/Resources.resw", encoding="utf-8") as f:
        lines = f.readlines()
        for i, line in enumerate(lines):
            if "QuickAccent_SelectedLanguage_" in line:
                LABELS.append(
                    line.split("SelectedLanguage_")[-1].split(".Content")[0]
                )  # 读取原始标签
                NAMESCN.append(
                    lines[i + 1].split("<value>")[-1].split("</value>")[0]
                )  # 读取翻译
                print(f"RESC {LABELS[-1]} >>> {NAMESCN[-1]}")

    with open("SettingsXAML/Views/PowerAccentPage.xaml", encoding="utf-8") as f:
        lines = f.readlines()
        for line in lines:
            if "QuickAccent_SelectedLanguage_" in line:
                VIEWS_ln.append(line)  # 读取View
                VLABLES.append(line.split("SelectedLanguage_")[-1].split('"')[0])
                print(f"VIEW {VLABLES[-1]} ==> {line.strip()}")

    with open("ViewModels/PowerAccentViewModel.cs", encoding="utf-8") as f:
        lines = f.readlines()
        for line in lines:
            if 4 < len(line.strip()) < 7 and line.upper() == line:
                MODELS_ln.append(line)  # 读取ViewModel
                print(f"VIEW {VLABLES[len(MODELS_ln)-1]} ==> {line.strip()}")
    return (
        dict(zip(LABELS, NAMESCN)),
        dict(zip(VLABLES, VIEWS_ln)),
        dict(zip(VLABLES, MODELS_ln)),
    )  # 返回原始文本与翻译的映射，以及需要替换的内容


def sort_label(dic):
    """将翻译按照拼音排序"""
    sorted_translations = sorted(
        (pinyin.get(cn, format="strip"), lab) for lab, cn in dic.items()
    )
    lst = ["All", "Currency", "Pinyin"]
    lst += [
        original_text
        for _, original_text in sorted_translations
        if original_text not in lst
    ]
    return {x: dic[x] for x in lst}


def replace_translations_in_file(file_path, lab2cn, sortedlab2cn, lab2xx):
    """在给定文件中替换原始文本为翻译后的文本"""
    with open(file_path, encoding="utf-8") as f:
        content = f.read()  # 读取文件内容
    temp = [
        f"00{i}"[-3:] + "XXXXAUTOTRANSLATIONXXXX" for i in range(len(lab2xx))
    ]  # 生成临时替换文本
    temp2lab = {tmp: txt for tmp, txt in zip(temp, lab2xx)}
    lab2sortedlab = {txt: sor for txt, sor in zip(lab2xx, sortedlab2cn)}
    temp2xx = {tmp: lab2xx[lab2sortedlab[temp2lab[tmp]]] for tmp in temp}

    for original, tmp in zip(lab2xx.values(), temp):
        content = content.replace(original, tmp)
    for tmp in temp:
        content = content.replace(tmp, temp2xx[tmp])
    # print(f"■■■■■■content: {content}■■■■■■")
    with open(file_path, "w", encoding="utf-8") as f:
        f.write(content)  # 写回文件


if __name__ == "__main__":
    main()  # 如果是作为脚本运行，调用 main 函数
