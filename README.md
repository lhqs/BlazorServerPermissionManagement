# Blazor Server 权限管理方案

欢迎来到 Blazor Server 权限管理方案项目！本项目旨在提供一个灵活且可扩展的权限管理解决方案，适用于各种 Web 应用程序。

## 目录

- [项目简介](#项目简介)
- [效果](#效果录频)
- [安装配置](#安装配置)
- [核心组件](#核心组件)
- [工作流程](#工作流程)
- [特点](#特点)
- [使用示例](#使用示例)
- [最佳实践](#最佳实践)
- [许可证](#许可证)

## 项目简介

Blazor Server 权限管理方案是一个基于 Blazor 的权限管理系统，支持多种权限类型和组件化的权限控制。该方案旨在帮助开发者轻松实现权限管理功能，确保应用程序的安全性和灵活性。

## 效果录频

以下是权限管理方案的效果视频：

<video width="640" height="480" controls>
    <source src="./permission-demo.mp4" type="video/mp4">
    您的浏览器不支持 HTML5 视频标签。
</video>


## 安装配置

本地环境： node.js 和 net9.0
数据库： PostgreSQL

启动前需要运行数据库脚本， 脚本内容在 init_postgres.sql

启动命令： dotnet run


## 核心组件

### 1. 权限模型（Permission）

权限模型包含以下关键属性：
- **ID**：权限唯一标识
- **名称（Name）**：权限名称
- **资源路径（ResourceUri）**：访问资源的路径
- **权限类型（Type）**：包括页面访问、数据访问、功能操作和 API 访问
- **描述（Description）**：权限说明

### 2. 授权视图组件（AuthorizeView）

用于包装需要权限控制的页面或组件，示例：
```razor
<ManagerSystem.Components.Auth.AuthorizeView Resource="/permissions" PermissionType="PermissionType.Page">
    <!-- 受保护的内容 -->
</ManagerSystem.Components.Auth.AuthorizeView>
```

### 3. 权限服务（IAuthService）

提供权限管理的核心服务接口，支持权限的 CRUD 操作。

## 工作流程

1. **权限定义**：在系统中预定义各种权限类型，为每个资源分配对应的权限。
2. **权限验证**：用户访问资源时，通过 `AuthorizeView` 组件进行权限检查。
3. **权限管理**：管理员通过权限管理界面维护权限信息。

## 特点

- **灵活的权限类型**：支持多种权限类型，满足不同场景需求。
- **组件化的权限控制**：使用 `AuthorizeView` 组件实现声明式权限控制。
- **完整的权限管理功能**：提供友好的管理界面，支持权限的增删改查操作。

## 使用示例

### 1. 页面权限控制
```razor
<ManagerSystem.Components.Auth.AuthorizeView Resource="/admin/dashboard" PermissionType="PermissionType.Page">
    <ChildContent>
        <button class="px-4 py-2 bg-blue-500 text-white rounded">有权限</button>
    </ChildContent>
    <UnauthorizedContent>
        <button class="px-4 py-2 bg-gray-300 text-gray-500 rounded" disabled>无权限</button>
    </UnauthorizedContent>
</ManagerSystem.Components.Auth.AuthorizeView>
```

### 2. 功能权限控制
```razor
<ManagerSystem.Components.Auth.AuthorizeView Resource="ExportData" PermissionType="PermissionType.Function">
    <ChildContent>
        <button class="px-4 py-2 bg-blue-500 text-white rounded">有权限</button>
    </ChildContent>
    <UnauthorizedContent>
        <button class="px-4 py-2 bg-gray-300 text-gray-500 rounded" disabled>无权限</button>
    </UnauthorizedContent>
</ManagerSystem.Components.Auth.AuthorizeView>

```

### 3. API权限控制
```razor
<ManagerSystem.Components.Auth.AuthorizeView Resource="/api/data" PermissionType="PermissionType.Api">
    <ChildContent>
        <button class="px-4 py-2 bg-blue-500 text-white rounded">有权限</button>
    </ChildContent>
    <UnauthorizedContent>
        <button class="px-4 py-2 bg-gray-300 text-gray-500 rounded" disabled>无权限</button>
    </UnauthorizedContent>
</ManagerSystem.Components.Auth.AuthorizeView>
```

### 4. Data权限控制
```razor
<ManagerSystem.Components.Auth.AuthorizeView Resource="/api/data" PermissionType="PermissionType.Data">
    <ChildContent>
        <button class="px-4 py-2 bg-blue-500 text-white rounded">有权限</button>
    </ChildContent>
    <UnauthorizedContent>
        <button class="px-4 py-2 bg-gray-300 text-gray-500 rounded" disabled>无权限</button>
    </UnauthorizedContent>
</ManagerSystem.Components.Auth.AuthorizeView>
```

## 最佳实践

- **权限粒度控制**：合理划分权限粒度，避免过细或过粗。
- **权限命名规范**：采用清晰的命名方式，保持权限命名的一致性。
- **权限缓存处理**：适当缓存权限数据，提高访问效率。
- **权限审计**：记录权限变更历史，定期审查权限配置。



## 许可证

本项目采用 MIT 许可证，详细信息请查看 [LICENSE](LICENSE) 文件。