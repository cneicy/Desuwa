#!/usr/bin/env python3
"""
从 DesuwaLogo.png 生成三个 ICO 文件
- 左半边 -> enabled.ico (启用状态)
- 右半边 -> disabled.ico (禁用状态)
- 左半边 -> app.ico (应用程序图标)

优化处理：
- 裁剪成 1:1 正方形比例
- 深灰色背景 (#343434) 转为透明
- 圆形居中

使用方法：
    cd Assets
    python generate_icons.py
"""

from PIL import Image
import os

def make_background_transparent(img, target_color=(52, 52, 52), tolerance=20):
    """
    将指定颜色的背景转为透明
    target_color: 目标背景色 RGB，默认 #343434 = (52, 52, 52)
    tolerance: 颜色容差
    """
    img = img.convert("RGBA")
    data = img.load()
    width, height = img.size

    target_r, target_g, target_b = target_color

    for y in range(height):
        for x in range(width):
            r, g, b, a = data[x, y]

            # 计算与目标颜色的距离
            color_distance = abs(r - target_r) + abs(g - target_g) + abs(b - target_b)

            # 如果颜色接近目标背景色，设为透明
            if color_distance <= tolerance * 3:  # 乘以3因为是三个通道的总和
                data[x, y] = (r, g, b, 0)

    return img

def crop_to_square_centered(img):
    """
    裁剪成正方形，保持内容居中
    """
    width, height = img.size

    # 计算正方形尺寸（取较小的边）
    size = min(width, height)

    # 计算居中裁剪的坐标
    left = (width - size) // 2
    top = (height - size) // 2
    right = left + size
    bottom = top + size

    return img.crop((left, top, right, bottom))

def process_half(img):
    """
    处理半边图片：裁剪正方形、透明背景
    """
    # 1. 裁剪成正方形，内容居中
    img = crop_to_square_centered(img)

    # 2. 背景透明化（针对 #343434 深灰色）
    img = make_background_transparent(img, target_color=(52, 52, 52), tolerance=25)

    return img

def generate_icons():
    # 获取脚本所在目录
    script_dir = os.path.dirname(os.path.abspath(__file__))

    # 输入输出路径（都在 Assets 目录）
    input_png = os.path.join(script_dir, "DesuwaLogo.png")
    enabled_ico = os.path.join(script_dir, "enabled.ico")
    disabled_ico = os.path.join(script_dir, "disabled.ico")
    app_ico = os.path.join(script_dir, "app.ico")

    # 读取原始图片
    print(f"📖 读取图片: {os.path.basename(input_png)}")
    img = Image.open(input_png)
    width, height = img.size
    print(f"   原始尺寸: {width}x{height}")
    print(f"   背景色: #343434 (RGB 52,52,52)")

    # 分割左右两半
    mid = width // 2

    # 左半边 - 启用状态（紫色圆形）
    left_half = img.crop((0, 0, mid, height))
    print(f"\n✂️  左半边 (启用): {mid}x{height}")

    # 右半边 - 禁用状态（禁止图标）
    right_half = img.crop((mid, 0, width, height))
    print(f"✂️  右半边 (禁用): {mid}x{height}")

    # 处理左半边（启用状态）
    print(f"\n🎨 处理启用状态图标...")
    left_processed = process_half(left_half)
    print(f"   处理后尺寸: {left_processed.size[0]}x{left_processed.size[1]} (1:1)")

    # 处理右半边（禁用状态）
    print(f"\n🎨 处理禁用状态图标...")
    right_processed = process_half(right_half)
    print(f"   处理后尺寸: {right_processed.size[0]}x{right_processed.size[1]} (1:1)")

    # 生成多尺寸 ICO（Windows 推荐尺寸）
    sizes = [(16, 16), (32, 32), (48, 48), (64, 64), (128, 128), (256, 256)]

    # 保存启用状态 ICO
    print(f"\n💾 生成: enabled.ico")
    left_processed.save(enabled_ico, format='ICO', sizes=sizes)
    print(f"   ✅ 包含尺寸: {', '.join([f'{s[0]}x{s[1]}' for s in sizes])}")

    # 保存禁用状态 ICO
    print(f"\n💾 生成: disabled.ico")
    right_processed.save(disabled_ico, format='ICO', sizes=sizes)
    print(f"   ✅ 包含尺寸: {', '.join([f'{s[0]}x{s[1]}' for s in sizes])}")

    # 应用图标使用启用状态
    print(f"\n💾 生成: app.ico (应用程序图标)")
    left_processed.save(app_ico, format='ICO', sizes=sizes)
    print(f"   ✅ 包含尺寸: {', '.join([f'{s[0]}x{s[1]}' for s in sizes])}")

    print("\n🎉 完成！生成了 3 个 ICO 文件:")
    print(f"   - enabled.ico  (托盘图标 - 启用)")
    print(f"   - disabled.ico (托盘图标 - 禁用)")
    print(f"   - app.ico      (应用程序图标)")
    print("\n✨ 优化处理:")
    print(f"   ✅ 1:1 正方形比例")
    print(f"   ✅ 深灰色背景 (#343434) 转透明")
    print(f"   ✅ 圆形居中")
    print("\n📝 提示：记得将生成的 ICO 文件提交到 Git")

if __name__ == "__main__":
    try:
        generate_icons()
    except FileNotFoundError as e:
        print(f"❌ 错误: 找不到文件")
        print(f"   {e}")
        print("   请确保在 Assets 目录下运行此脚本")
    except Exception as e:
        print(f"❌ 错误: {e}")
        import traceback
        traceback.print_exc()


