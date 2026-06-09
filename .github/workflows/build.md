# Build & Release Workflow

## 📋 概述

CI/CD 流程完全由 **commit message 关键词** 驱动。推送到 `main` 或 `master` 分支时，GitHub Actions 会根据关键词自动执行相应操作。

## 🔑 关键词

| Commit message 中的关键词 | 构建 (Windows x64) | GitHub Release |
|---------------------------|:---:|:---:|
| `build action` | ✅ | ❌ |
| `build release` | ✅ | ✅ |

> **注意：** Pull Request 始终触发构建（不发布 Release）。关键词对 PR 无效。

## 🚀 使用示例

```bash
# ============================================================
# 仅构建，验证编译是否成功
# ============================================================
git commit --allow-empty -m "ci: test build (build action)"
git push

# ============================================================
# 构建 + 创建 GitHub Release
# ============================================================
git commit -m "release: v0.1.0 (build release)"
git push

# ============================================================
# 普通提交（不触发构建）
# ============================================================
git commit -m "docs: update README"
git push

git commit -m "fix: resolve UI rendering issue"
git push

git commit -m "feat: add new feature"
git push
```

## 🏗️ 构建目标

| 版本 | 平台 | 架构 | 运行时 | 体积 | 说明 |
|------|------|:---:|--------|:----:|------|
| Self-contained | Windows | x64 | 内置 | ~140MB | 单文件 exe，无需安装 .NET 运行时，双击即可运行 |
| Runtime-dependent | Windows | x64 | 需要 .NET 8.0 | ~60KB | 压缩包，需要预先安装 .NET 8.0 运行时 |

## 📦 流程阶段

```
check ──→ build ──→ release
  │         │         │
  │         │         └─ 下载构建产物
  │         │            删除旧的 release/tag
  │         │            生成 release notes
  │         │            创建 GitHub Release（包含两个版本）
  │         │
  │         └─ 编译 Windows x64 self-contained
  │            编译 Windows x64 green 版本
  │            上传构建产物
  │
  └─ 解析 commit message
     从 Desuwa.csproj 提取版本号
```

```mermaid
flowchart TB
    subgraph check["check"]
        C1[解析 commit message]
        C2[从 Desuwa.csproj 提取版本号]
    end

    subgraph build["build"]
        B1[编译 Windows x64 self-contained]
        B2[上传构建产物]
    end

    subgraph release["release"]
        R1[下载构建产物]
        R2[删除旧的 release/tag]
        R3[生成 release notes]
        R4[创建 GitHub Release]
    end

    C1 --> C2
    C2 --> B1
    B1 --> B2
    B2 --> R1
    R1 --> R2 --> R3 --> R4
```

## 📌 版本号

版本号从 `Desuwa.csproj` 中的 `<Version>` 标签自动提取，用于：
- Release tag 名称（例如 `v0.1.0`）
- 产物文件名（例如 `Desuwa-windows-x64-self-contained-v0.1.0.exe`、`Desuwa-windows-x64-runtime-dependent-v0.1.0.zip`）
- exe 文件属性中的版本信息

### 如何更新版本号

编辑 `Desuwa.csproj` 文件：

```xml
<PropertyGroup>
    <!-- 修改这里的版本号 -->
    <Version>0.2.0</Version>
    <AssemblyVersion>0.2.0.0</AssemblyVersion>
    <FileVersion>0.2.0.0</FileVersion>
    ...
</PropertyGroup>
```

然后提交并触发构建：

```bash
git add Desuwa.csproj
git commit -m "release: v0.2.0 (build release)"
git push
```

## 🎯 两种发布模式

### Self-contained 版本（推荐）
- ✅ 单个 exe 文件，无需安装 .NET 运行时
- ✅ 可在没有 .NET 的 Windows 系统上直接运行
- ✅ 所有依赖项都打包在 exe 中
- ⚠️ 体积较大（约 140MB）

### Green 绿色版
- ✅ 体积小巧（约 25KB）
- ✅ 适合已安装 .NET 运行时的用户
- ⚠️ 需要预先安装 .NET 8.0 运行时
- ⚠️ 以 zip 压缩包形式发布

### 本地测试

```bash
cd D:\aaaStuffsaaa\from_git\github\Desuwa

# Self-contained 版本
dotnet publish -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish-selfcontained
# 输出: publish-selfcontained\Desuwa.exe

# Runtime-dependent 版本
dotnet publish -c Release --self-contained false -o publish-runtime-dependent
# 输出: publish-runtime-dependent\Desuwa.exe (需要 .NET 运行时)
```
