# Assets 资源文件

## 图标文件

### DesuwaLogo.png
- 原始 logo 图片（由 nano banana 生成）
- 左半边：启用状态图标
- 右半边：禁用状态图标

### 生成的 ICO 文件

这些 ICO 文件已提交到仓库，GitHub Actions 构建时会直接使用：

1. **app.ico** - 应用程序图标（exe 文件图标）
   - 使用左半边（启用状态）
   - 在 Windows 资源管理器中显示

2. **enabled.ico** - 托盘图标（启用状态）
   - 使用左半边
   - 程序启用时在系统托盘显示

3. **disabled.ico** - 托盘图标（禁用状态）
   - 使用右半边
   - 程序禁用时在系统托盘显示

## 重新生成图标

如果需要更新 logo，运行以下命令重新生成 ICO 文件：

```bash
# 进入 Assets 目录
cd Assets

# 安装依赖（首次运行）
uv venv
uv pip install Pillow

# 运行脚本
uv run python generate_icons.py
#别忘了 add commit push 生成的ico文件
```

## 图标在代码中的使用

- **app.ico**: 在 `Desuwa.csproj` 中配置为 `<ApplicationIcon>`
- **enabled.ico / disabled.ico**: 在 `Program.cs` 中作为嵌入资源加载
