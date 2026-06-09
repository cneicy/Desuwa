#!/usr/bin/env python3
"""
自动更新 Desuwa.csproj 中的版本号

用法:
    python bump.py --version 1.0.1
    python bump.py -v 1.0.1
"""

import argparse
import re
import sys
from pathlib import Path


def update_csproj_version(csproj_path: Path, new_version: str) -> bool:
    """更新 .csproj 文件中的版本号"""

    if not csproj_path.exists():
        print(f"❌ 错误: 找不到文件 {csproj_path}")
        return False

    # 读取文件内容
    content = csproj_path.read_text(encoding='utf-8')

    # 验证版本号格式 (支持 1.0.0 或 1.0.0.0)
    version_pattern = r'^\d+\.\d+\.\d+(\.\d+)?$'
    if not re.match(version_pattern, new_version):
        print(f"❌ 错误: 版本号格式无效 '{new_version}'")
        print("   支持的格式: 1.0.0 或 1.0.0.0")
        return False

    # 准备版本号 (确保有 4 位)
    version_parts = new_version.split('.')
    if len(version_parts) == 3:
        version_3 = new_version  # 1.0.0
        version_4 = f"{new_version}.0"  # 1.0.0.0
    else:
        version_4 = new_version  # 1.0.0.0
        version_3 = '.'.join(version_parts[:3])  # 1.0.0

    # 替换版本号
    patterns = [
        (r'<Version>[\d.]+</Version>', f'<Version>{version_3}</Version>'),
        (r'<AssemblyVersion>[\d.]+</AssemblyVersion>', f'<AssemblyVersion>{version_4}</AssemblyVersion>'),
        (r'<FileVersion>[\d.]+</FileVersion>', f'<FileVersion>{version_4}</FileVersion>'),
    ]

    updated_content = content
    changes_made = []

    for pattern, replacement in patterns:
        match = re.search(pattern, updated_content)
        if match:
            old_value = match.group(0)
            updated_content = re.sub(pattern, replacement, updated_content)
            changes_made.append(f"  {old_value} → {replacement}")
        else:
            tag_name = pattern.split('<')[1].split('>')[0]
            print(f"⚠️  警告: 未找到 <{tag_name}> 标签")

    if not changes_made:
        print("❌ 错误: 未找到任何版本号标签")
        return False

    # 写回文件
    csproj_path.write_text(updated_content, encoding='utf-8')

    print(f"✅ 成功更新版本号到 {version_3}")
    print("\n更改内容:")
    for change in changes_made:
        print(change)

    return True


def main():
    parser = argparse.ArgumentParser(
        description='自动更新 Desuwa.csproj 中的版本号',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
示例:
  python bump.py --version 1.0.1
  python bump.py -v 2.0.0
  python bump.py --version 1.2.3.4
        """
    )

    parser.add_argument(
        '-v', '--version',
        required=True,
        help='新版本号 (格式: 1.0.0 或 1.0.0.0)'
    )

    args = parser.parse_args()

    # 查找 .csproj 文件
    script_dir = Path(__file__).parent
    csproj_path = script_dir / 'Desuwa.csproj'

    print(f"📦 项目文件: {csproj_path}")
    print(f"🎯 目标版本: {args.version}")
    print()

    # 更新版本号
    success = update_csproj_version(csproj_path, args.version)

    if success:
        print()
        print("💡 提示: 记得提交更改并推送到 GitHub")
        print(f"   git add Desuwa.csproj")
        print(f"   git commit -m \"release: v{args.version} (build release)\"")
        print(f"   git push")
        sys.exit(0)
    else:
        sys.exit(1)


if __name__ == '__main__':
    main()
