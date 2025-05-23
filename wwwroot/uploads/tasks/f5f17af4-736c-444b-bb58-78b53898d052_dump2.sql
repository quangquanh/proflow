USE [master]
GO
/****** Object:  Database [WatchStoreDB]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE DATABASE [WatchStoreDB]
 CONTAINMENT = NONE
 ON  PRIMARY 
( NAME = N'WatchStoreDB', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\WatchStoreDB.mdf' , SIZE = 8192KB , MAXSIZE = UNLIMITED, FILEGROWTH = 65536KB )
 LOG ON 
( NAME = N'WatchStoreDB_log', FILENAME = N'C:\Program Files\Microsoft SQL Server\MSSQL16.SQLEXPRESS\MSSQL\DATA\WatchStoreDB_log.ldf' , SIZE = 8192KB , MAXSIZE = 2048GB , FILEGROWTH = 65536KB )
 WITH CATALOG_COLLATION = DATABASE_DEFAULT, LEDGER = OFF
GO
ALTER DATABASE [WatchStoreDB] SET COMPATIBILITY_LEVEL = 160
GO
IF (1 = FULLTEXTSERVICEPROPERTY('IsFullTextInstalled'))
begin
EXEC [WatchStoreDB].[dbo].[sp_fulltext_database] @action = 'enable'
end
GO
ALTER DATABASE [WatchStoreDB] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [WatchStoreDB] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [WatchStoreDB] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [WatchStoreDB] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [WatchStoreDB] SET ARITHABORT OFF 
GO
ALTER DATABASE [WatchStoreDB] SET AUTO_CLOSE ON 
GO
ALTER DATABASE [WatchStoreDB] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [WatchStoreDB] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [WatchStoreDB] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [WatchStoreDB] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [WatchStoreDB] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [WatchStoreDB] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [WatchStoreDB] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [WatchStoreDB] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [WatchStoreDB] SET  ENABLE_BROKER 
GO
ALTER DATABASE [WatchStoreDB] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [WatchStoreDB] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [WatchStoreDB] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [WatchStoreDB] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [WatchStoreDB] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [WatchStoreDB] SET READ_COMMITTED_SNAPSHOT ON 
GO
ALTER DATABASE [WatchStoreDB] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [WatchStoreDB] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [WatchStoreDB] SET  MULTI_USER 
GO
ALTER DATABASE [WatchStoreDB] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [WatchStoreDB] SET DB_CHAINING OFF 
GO
ALTER DATABASE [WatchStoreDB] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [WatchStoreDB] SET TARGET_RECOVERY_TIME = 60 SECONDS 
GO
ALTER DATABASE [WatchStoreDB] SET DELAYED_DURABILITY = DISABLED 
GO
ALTER DATABASE [WatchStoreDB] SET ACCELERATED_DATABASE_RECOVERY = OFF  
GO
ALTER DATABASE [WatchStoreDB] SET QUERY_STORE = ON
GO
ALTER DATABASE [WatchStoreDB] SET QUERY_STORE (OPERATION_MODE = READ_WRITE, CLEANUP_POLICY = (STALE_QUERY_THRESHOLD_DAYS = 30), DATA_FLUSH_INTERVAL_SECONDS = 900, INTERVAL_LENGTH_MINUTES = 60, MAX_STORAGE_SIZE_MB = 1000, QUERY_CAPTURE_MODE = AUTO, SIZE_BASED_CLEANUP_MODE = AUTO, MAX_PLANS_PER_QUERY = 200, WAIT_STATS_CAPTURE_MODE = ON)
GO
USE [WatchStoreDB]
GO
/****** Object:  Table [dbo].[__EFMigrationsHistory]    Script Date: 4/22/2025 1:04:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[__EFMigrationsHistory](
	[MigrationId] [nvarchar](150) NOT NULL,
	[ProductVersion] [nvarchar](32) NOT NULL,
 CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY CLUSTERED 
(
	[MigrationId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CartItems]    Script Date: 4/22/2025 1:04:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CartItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[CartId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[AddedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_CartItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Carts]    Script Date: 4/22/2025 1:04:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Carts](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[UpdatedAt] [datetime2](7) NOT NULL,
 CONSTRAINT [PK_Carts] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Categories]    Script Date: 4/22/2025 1:04:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Categories](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](255) NULL,
 CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[OrderItems]    Script Date: 4/22/2025 1:04:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrderItems](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[OrderId] [int] NOT NULL,
	[ProductId] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
	[UnitPrice] [decimal](18, 2) NOT NULL,
	[Subtotal] [decimal](18, 2) NOT NULL,
 CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Orders]    Script Date: 4/22/2025 1:04:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Orders](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [int] NOT NULL,
	[OrderDate] [datetime2](7) NOT NULL,
	[TotalAmount] [decimal](18, 2) NOT NULL,
	[Status] [nvarchar](20) NOT NULL,
	[ShippingAddress] [nvarchar](255) NULL,
	[PhoneNumber] [nvarchar](15) NULL,
	[PaymentMethod] [nvarchar](100) NULL,
	[IsPaid] [bit] NOT NULL,
	[PaidDate] [datetime2](7) NULL,
	[Address] [nvarchar](200) NOT NULL,
	[City] [nvarchar](100) NOT NULL,
	[Country] [nvarchar](50) NOT NULL,
	[FullName] [nvarchar](100) NOT NULL,
	[Notes] [nvarchar](max) NULL,
	[PostalCode] [nvarchar](20) NOT NULL,
	[State] [nvarchar](50) NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Products]    Script Date: 4/22/2025 1:04:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Products](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](100) NOT NULL,
	[Description] [nvarchar](500) NOT NULL,
	[Price] [decimal](18, 2) NOT NULL,
	[CategoryId] [int] NOT NULL,
	[ImageUrl] [nvarchar](200) NULL,
	[StockQuantity] [int] NOT NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[Brand] [nvarchar](100) NULL,
	[Model] [nvarchar](100) NULL,
	[Gender] [nvarchar](50) NULL,
	[MovementType] [nvarchar](50) NULL,
	[CaseSize] [nvarchar](50) NULL,
	[CaseMaterial] [nvarchar](50) NULL,
	[BandMaterial] [nvarchar](50) NULL,
	[WaterResistance] [nvarchar](50) NULL,
	[IsAvailable] [bit] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Users]    Script Date: 4/22/2025 1:04:58 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Users](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Username] [nvarchar](100) NOT NULL,
	[Password] [nvarchar](100) NOT NULL,
	[Email] [nvarchar](100) NOT NULL,
	[Role] [nvarchar](max) NOT NULL,
	[FullName] [nvarchar](100) NULL,
	[PhoneNumber] [nvarchar](15) NULL,
	[Address] [nvarchar](255) NULL,
	[CreatedAt] [datetime2](7) NOT NULL,
	[IsAdmin] [bit] NOT NULL,
	[UpdatedAt] [datetime2](7) NULL,
 CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250418174219_InitialCreate', N'9.0.4')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250419174416_AddIsAvailableAndUpdatedAtToProducts', N'9.0.4')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250421034322_AddOrderItemsTable', N'9.0.4')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250421040225_AddUpdatedAtToOrders', N'9.0.4')
INSERT [dbo].[__EFMigrationsHistory] ([MigrationId], [ProductVersion]) VALUES (N'20250421041406_AddUpdatedAtToUsers', N'9.0.4')
GO
SET IDENTITY_INSERT [dbo].[Carts] ON 

INSERT [dbo].[Carts] ([Id], [UserId], [CreatedAt], [UpdatedAt]) VALUES (1, 1, CAST(N'2025-04-19T00:52:39.2468352' AS DateTime2), CAST(N'2025-04-19T00:52:39.2468359' AS DateTime2))
INSERT [dbo].[Carts] ([Id], [UserId], [CreatedAt], [UpdatedAt]) VALUES (2, 2, CAST(N'2025-04-20T00:09:24.1893314' AS DateTime2), CAST(N'2025-04-20T00:09:24.1893327' AS DateTime2))
SET IDENTITY_INSERT [dbo].[Carts] OFF
GO
SET IDENTITY_INSERT [dbo].[Categories] ON 

INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (1, N'Analog', N'Đồng hồ kim truyền thống')
INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (2, N'Digital', N'Đồng hồ điện tử')
INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (3, N'Smart', N'Đồng hồ thông minh')
INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (4, N'Automatic', N'Đồng hồ tự động')
INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (5, N'Luxury', N'Đồng hồ cao cấp')
INSERT [dbo].[Categories] ([Id], [Name], [Description]) VALUES (6, N'test', N'testt')
SET IDENTITY_INSERT [dbo].[Categories] OFF
GO
SET IDENTITY_INSERT [dbo].[OrderItems] ON 

INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (1, 2, 11, 1, CAST(12000000.00 AS Decimal(18, 2)), CAST(12000000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (2, 3, 11, 1, CAST(12000000.00 AS Decimal(18, 2)), CAST(12000000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (3, 3, 9, 1, CAST(2000000.00 AS Decimal(18, 2)), CAST(2000000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (4, 4, 9, 1, CAST(2000000.00 AS Decimal(18, 2)), CAST(2000000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (5, 5, 25, 1, CAST(1200000000.00 AS Decimal(18, 2)), CAST(1200000000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (6, 6, 26, 1, CAST(1000000.00 AS Decimal(18, 2)), CAST(1000000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (7, 7, 3, 1, CAST(3500000.00 AS Decimal(18, 2)), CAST(3500000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (8, 8, 27, 1, CAST(10000000.00 AS Decimal(18, 2)), CAST(10000000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (9, 9, 24, 1, CAST(120000000.00 AS Decimal(18, 2)), CAST(120000000.00 AS Decimal(18, 2)))
INSERT [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [Quantity], [UnitPrice], [Subtotal]) VALUES (10, 10, 11, 2, CAST(12000000.00 AS Decimal(18, 2)), CAST(24000000.00 AS Decimal(18, 2)))
SET IDENTITY_INSERT [dbo].[OrderItems] OFF
GO
SET IDENTITY_INSERT [dbo].[Orders] ON 

INSERT [dbo].[Orders] ([Id], [UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PhoneNumber], [PaymentMethod], [IsPaid], [PaidDate], [Address], [City], [Country], [FullName], [Notes], [PostalCode], [State], [UpdatedAt]) VALUES (2, 2, CAST(N'2025-04-21T10:21:36.9292358' AS DateTime2), CAST(12000000.00 AS Decimal(18, 2)), N'Delivered', NULL, N'0978801140', NULL, 0, NULL, N'Canh Nậu', N'Hà Nội', N'Vietnam', N'customer1', NULL, N'846764', N'Hà Tây', CAST(N'2025-04-21T13:01:12.2472478' AS DateTime2))
INSERT [dbo].[Orders] ([Id], [UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PhoneNumber], [PaymentMethod], [IsPaid], [PaidDate], [Address], [City], [Country], [FullName], [Notes], [PostalCode], [State], [UpdatedAt]) VALUES (3, 1, CAST(N'2025-03-21T10:39:47.1893360' AS DateTime2), CAST(14000000.00 AS Decimal(18, 2)), N'Delivered', NULL, N'0978801140', NULL, 0, NULL, N'Canh Nậu', N'Hà Nội', N'Vietnam', N'admin', NULL, N'846764', N'Hà Tây', CAST(N'2025-04-21T12:51:27.7066264' AS DateTime2))
INSERT [dbo].[Orders] ([Id], [UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PhoneNumber], [PaymentMethod], [IsPaid], [PaidDate], [Address], [City], [Country], [FullName], [Notes], [PostalCode], [State], [UpdatedAt]) VALUES (4, 1, CAST(N'2025-04-21T12:59:52.6986234' AS DateTime2), CAST(2000000.00 AS Decimal(18, 2)), N'Delivered', NULL, N'0978801140', NULL, 0, NULL, N'Canh Nậu', N'Hà Nội', N'Vietnam', N'admin', NULL, N'846764', N'Hà Tây', CAST(N'2025-04-21T13:01:08.8740189' AS DateTime2))
INSERT [dbo].[Orders] ([Id], [UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PhoneNumber], [PaymentMethod], [IsPaid], [PaidDate], [Address], [City], [Country], [FullName], [Notes], [PostalCode], [State], [UpdatedAt]) VALUES (5, 1, CAST(N'2025-04-21T13:03:58.0191548' AS DateTime2), CAST(1200000000.00 AS Decimal(18, 2)), N'Delivered', NULL, N'0978801140', NULL, 0, NULL, N'Canh Nậu', N'Hà Nội', N'Vietnam', N'abcabc', NULL, N'846764', N'Hà Tây', CAST(N'2025-04-21T14:04:25.0281847' AS DateTime2))
INSERT [dbo].[Orders] ([Id], [UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PhoneNumber], [PaymentMethod], [IsPaid], [PaidDate], [Address], [City], [Country], [FullName], [Notes], [PostalCode], [State], [UpdatedAt]) VALUES (6, 1, CAST(N'2025-04-21T16:06:05.0485319' AS DateTime2), CAST(1000000.00 AS Decimal(18, 2)), N'Pending', NULL, N'0978801140', NULL, 0, NULL, N'Canh Nậu', N'Hà Nội', N'Vietnam', N'admin', NULL, N'846764', N'Hà Tây', NULL)
INSERT [dbo].[Orders] ([Id], [UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PhoneNumber], [PaymentMethod], [IsPaid], [PaidDate], [Address], [City], [Country], [FullName], [Notes], [PostalCode], [State], [UpdatedAt]) VALUES (7, 1, CAST(N'2025-04-21T16:23:05.9701395' AS DateTime2), CAST(3500000.00 AS Decimal(18, 2)), N'Pending', NULL, N'0978801140', NULL, 0, NULL, N'Canh Nậu', N'Hà Nội', N'Vietnam', N'admin', NULL, N'846764', N'Hà Tây', NULL)
INSERT [dbo].[Orders] ([Id], [UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PhoneNumber], [PaymentMethod], [IsPaid], [PaidDate], [Address], [City], [Country], [FullName], [Notes], [PostalCode], [State], [UpdatedAt]) VALUES (8, 1, CAST(N'2025-04-22T11:49:50.3808157' AS DateTime2), CAST(10000000.00 AS Decimal(18, 2)), N'Pending', NULL, N'0978801140', NULL, 0, NULL, N'Canh Nậu', N'Hà Nội', N'Vietnam', N'admin', NULL, N'846764', N'Hà Tây', NULL)
INSERT [dbo].[Orders] ([Id], [UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PhoneNumber], [PaymentMethod], [IsPaid], [PaidDate], [Address], [City], [Country], [FullName], [Notes], [PostalCode], [State], [UpdatedAt]) VALUES (9, 2, CAST(N'2025-04-22T11:52:50.6422110' AS DateTime2), CAST(120000000.00 AS Decimal(18, 2)), N'Delivered', NULL, N'0978801140', NULL, 0, NULL, N'Canh Nậu', N'Hà Nội', N'Vietnam', N'customer1', NULL, N'846764', N'Hà Tây', CAST(N'2025-04-22T11:56:45.3957745' AS DateTime2))
INSERT [dbo].[Orders] ([Id], [UserId], [OrderDate], [TotalAmount], [Status], [ShippingAddress], [PhoneNumber], [PaymentMethod], [IsPaid], [PaidDate], [Address], [City], [Country], [FullName], [Notes], [PostalCode], [State], [UpdatedAt]) VALUES (10, 1, CAST(N'2025-04-22T12:33:04.0198617' AS DateTime2), CAST(24000000.00 AS Decimal(18, 2)), N'Pending', NULL, N'0978801140', NULL, 0, NULL, N'Canh Nậu', N'Hà Nội', N'Vietnam', N'admin', NULL, N'846764', N'Hà Tây', NULL)
SET IDENTITY_INSERT [dbo].[Orders] OFF
GO
SET IDENTITY_INSERT [dbo].[Products] ON 

INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (1, N'Seiko Presage', N'Đồng hồ cơ tự động cao cấp', CAST(8500000.00 AS Decimal(18, 2)), 1, N'/images/products/7150eed8-e18d-41d5-87b6-f972ae70d227.jpg', 10, CAST(N'2025-04-19T00:43:47.3023852' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (2, N'Tissot Classic Dream', N'Đồng hồ kim thanh lịch', CAST(4500000.00 AS Decimal(18, 2)), 1, N'/images/products/f57270db-c6a2-4a87-b109-df923a9f2555.jpg', 15, CAST(N'2025-04-19T00:43:47.3025626' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (3, N'Citizen Eco-Drive', N'Đồng hồ năng lượng ánh sáng', CAST(3500000.00 AS Decimal(18, 2)), 1, N'/images/products/9c6651e3-0ac0-40d0-b235-72f1bd1ea754.jpg', 19, CAST(N'2025-04-19T00:43:47.3025662' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (4, N'Orient Bambino', N'Đồng hồ cơ giá rẻ chất lượng', CAST(2500000.00 AS Decimal(18, 2)), 1, N'/images/products/2af044f2-271c-4430-bdb4-b1c4327121f6.jpg', 12, CAST(N'2025-04-19T00:43:47.3025666' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (5, N'Longines Master Collection', N'Đồng hồ cao cấp', CAST(15000000.00 AS Decimal(18, 2)), 1, N'/images/products/f80dddd2-e57f-4752-9d5c-1ec2c8de8282.jpg', 5, CAST(N'2025-04-19T00:43:47.3025669' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (6, N'Casio G-Shock', N'Đồng hồ thể thao chống sốc', CAST(2500000.00 AS Decimal(18, 2)), 2, N'/images/products/735f74c8-dec7-49bd-b9d5-26fe0b167311.jpg', 25, CAST(N'2025-04-19T00:43:47.3025671' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (7, N'Casio Edifice', N'Đồng hồ thể thao cao cấp', CAST(3500000.00 AS Decimal(18, 2)), 2, N'/images/products/7b2f5fc2-a033-4222-b2a0-5d2fc88ffc7a.jpg', 15, CAST(N'2025-04-19T00:43:47.3025674' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (8, N'Timex Ironman', N'Đồng hồ thể thao đa năng', CAST(1500000.00 AS Decimal(18, 2)), 2, N'/images/products/f872f9a6-8522-4f7b-b9d1-3f9e79233628.jpg', 20, CAST(N'2025-04-19T00:43:47.3025676' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (9, N'Casio Baby-G', N'Đồng hồ thể thao nữ', CAST(2000000.00 AS Decimal(18, 2)), 2, N'/images/products/94fba811-d69c-4fce-aa4a-813f229d5f9a.jpg', 16, CAST(N'2025-04-19T00:43:47.3025679' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (10, N'Casio Pro Trek', N'Đồng hồ leo núi', CAST(4500000.00 AS Decimal(18, 2)), 2, N'/images/products/8834294f-bd29-4baf-90f9-d678064b1d1c.jpg', 10, CAST(N'2025-04-19T00:43:47.3025681' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (11, N'Apple Watch Series 9', N'Đồng hồ thông minh cao cấp', CAST(12000000.00 AS Decimal(18, 2)), 3, N'/images/products/fd132282-eb4d-451c-8613-2f8f10ccdddb.jpg', 11, CAST(N'2025-04-19T00:43:47.3025693' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (12, N'Samsung Galaxy Watch 6', N'Đồng hồ thông minh Android', CAST(8000000.00 AS Decimal(18, 2)), 3, N'/images/products/c071d615-4518-4c61-91c0-c60801dad60f.jpg', 20, CAST(N'2025-04-19T00:43:47.3025696' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (13, N'Fitbit Versa 4', N'Đồng hồ theo dõi sức khỏe', CAST(6000000.00 AS Decimal(18, 2)), 3, N'/images/products/3b46c0e0-64fd-4778-96bf-539bf665badb.jpg', 25, CAST(N'2025-04-19T00:43:47.3025699' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (14, N'Garmin Fenix 7', N'Đồng hồ thể thao cao cấp', CAST(15000000.00 AS Decimal(18, 2)), 3, N'/images/products/7b324e9d-3efb-4af5-9ecf-752a7c058308.jpg', 10, CAST(N'2025-04-19T00:43:47.3025701' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (15, N'Xiaomi Mi Band 8', N'Vòng đeo tay thông minh', CAST(1500000.00 AS Decimal(18, 2)), 3, N'/images/products/1ffe81c7-855f-4231-8f29-c43ffcd9d683.jpg', 30, CAST(N'2025-04-19T00:43:47.3025704' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (16, N'Seiko 5 Sports', N'Đồng hồ cơ tự động', CAST(3500000.00 AS Decimal(18, 2)), 4, N'/images/products/a1479022-6de8-41b8-a151-0663f7d65155.jpg', 20, CAST(N'2025-04-19T00:43:47.3025706' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (17, N'Orient Kamasu', N'Đồng hồ lặn tự động', CAST(4500000.00 AS Decimal(18, 2)), 4, N'/images/products/7ddbbb3e-1fb9-40c7-89b1-5a1bcc64bbbf.jpg', 15, CAST(N'2025-04-19T00:43:47.3025709' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (18, N'Tissot Powermatic 80', N'Đồng hồ cơ tự động', CAST(8500000.00 AS Decimal(18, 2)), 4, N'/images/products/3a2c4f2d-6c89-4eb7-a65a-71f11db6c807.jpg', 10, CAST(N'2025-04-19T00:43:47.3025711' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (19, N'Hamilton Khaki Field', N'Đồng hồ quân đội', CAST(12000000.00 AS Decimal(18, 2)), 4, N'/images/products/6fc1b257-d894-4730-a150-4cedc0c362e6.jpg', 8, CAST(N'2025-04-19T00:43:47.3025714' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (20, N'Mido Multifort', N'Đồng hồ cơ tự động', CAST(9500000.00 AS Decimal(18, 2)), 4, N'/images/products/7d88b98f-76c1-451f-b4c0-1c6d18b6623c.jpg', 12, CAST(N'2025-04-19T00:43:47.3025717' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (21, N'Rolex Submariner', N'Đồng hồ lặn huyền thoại', CAST(850000000.00 AS Decimal(18, 2)), 5, N'/images/products/8268f5c0-d2fc-4cec-9236-76020ea2251a.jpg', 2, CAST(N'2025-04-19T00:43:47.3025720' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (22, N'Omega Speedmaster', N'Đồng hồ phi hành gia', CAST(250000000.00 AS Decimal(18, 2)), 5, N'/images/products/1b8e26ea-ce09-4e86-a12d-a13be25c978e.jpg', 3, CAST(N'2025-04-19T00:43:47.3025724' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (23, N'TAG Heuer Carrera', N'Đồng hồ đua xe', CAST(85000000.00 AS Decimal(18, 2)), 5, N'/images/products/178aff5c-3b53-4a63-991e-55fba7353414.jpg', 5, CAST(N'2025-04-19T00:43:47.3025738' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (24, N'IWC Portugieser', N'Đồng hồ cao cấp', CAST(120000000.00 AS Decimal(18, 2)), 5, N'/images/products/a92d8ae8-9a1d-46f5-b4d2-b0caec631274.jpg', 2, CAST(N'2025-04-19T00:43:47.3025740' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (25, N'Patek Philippe Nautilus', N'Đồng hồ siêu sang', CAST(1200000000.00 AS Decimal(18, 2)), 5, N'/images/products/c8e78dd9-6697-4271-bb53-711d85f7e774.jpg', 0, CAST(N'2025-04-19T00:43:47.3025743' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 0, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (26, N'đồng hồ test', N'đồng hồ test', CAST(1000000.00 AS Decimal(18, 2)), 6, N'/images/products/0b818dc9-b6d3-4f81-b2f4-93d48774d355.jpg', 998, CAST(N'2025-04-21T12:58:31.4603026' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL)
INSERT [dbo].[Products] ([Id], [Name], [Description], [Price], [CategoryId], [ImageUrl], [StockQuantity], [CreatedAt], [Brand], [Model], [Gender], [MovementType], [CaseSize], [CaseMaterial], [BandMaterial], [WaterResistance], [IsAvailable], [UpdatedAt]) VALUES (27, N'product test', N'product test', CAST(10000000.00 AS Decimal(18, 2)), 6, N'/images/products/7dac7ea6-face-4c31-b836-80ebfb5bcebc.jpg', 99, CAST(N'2025-04-22T11:48:41.6476438' AS DateTime2), NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, 1, NULL)
SET IDENTITY_INSERT [dbo].[Products] OFF
GO
SET IDENTITY_INSERT [dbo].[Users] ON 

INSERT [dbo].[Users] ([Id], [Username], [Password], [Email], [Role], [FullName], [PhoneNumber], [Address], [CreatedAt], [IsAdmin], [UpdatedAt]) VALUES (1, N'admin', N'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', N'admin@watchstore.com', N'Admin', N'Administrator', NULL, NULL, CAST(N'2025-04-19T00:43:47.2693192' AS DateTime2), 1, NULL)
INSERT [dbo].[Users] ([Id], [Username], [Password], [Email], [Role], [FullName], [PhoneNumber], [Address], [CreatedAt], [IsAdmin], [UpdatedAt]) VALUES (2, N'customer1', N'i7DPbrmxfQ99IrRW8SElfcElTh8BZlNwR2OD6ndt9BQ=', N'customer1@email.com', N'Customer', N'Nguyễn Văn A', N'0978801140', NULL, CAST(N'2025-04-19T00:43:47.2693489' AS DateTime2), 0, CAST(N'2025-04-21T11:53:44.4304654' AS DateTime2))
INSERT [dbo].[Users] ([Id], [Username], [Password], [Email], [Role], [FullName], [PhoneNumber], [Address], [CreatedAt], [IsAdmin], [UpdatedAt]) VALUES (3, N'customer2', N'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', N'customer2@email.com', N'Customer', N'Trần Thị B', NULL, NULL, CAST(N'2025-04-19T00:43:47.2693491' AS DateTime2), 0, NULL)
INSERT [dbo].[Users] ([Id], [Username], [Password], [Email], [Role], [FullName], [PhoneNumber], [Address], [CreatedAt], [IsAdmin], [UpdatedAt]) VALUES (6, N'customer3', N'jZae727K08KaOmKSgOaGzww/XVqGr/PKEgIMkjrcbJI=', N'customer3@email.com', N'Customer', NULL, NULL, NULL, CAST(N'2025-04-21T13:39:16.1028536' AS DateTime2), 0, NULL)
INSERT [dbo].[Users] ([Id], [Username], [Password], [Email], [Role], [FullName], [PhoneNumber], [Address], [CreatedAt], [IsAdmin], [UpdatedAt]) VALUES (8, N'admin2', N'123456', N'sv1@gmail.com', N'Admin', N'ĐỖ QUANG', N'0978801140', N'Canh Nậu', CAST(N'2025-04-22T12:50:03.4240353' AS DateTime2), 0, NULL)
SET IDENTITY_INSERT [dbo].[Users] OFF
GO
/****** Object:  Index [IX_CartItems_CartId]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE NONCLUSTERED INDEX [IX_CartItems_CartId] ON [dbo].[CartItems]
(
	[CartId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_CartItems_ProductId]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE NONCLUSTERED INDEX [IX_CartItems_ProductId] ON [dbo].[CartItems]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Carts_UserId]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Carts_UserId] ON [dbo].[Carts]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItems_OrderId]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE NONCLUSTERED INDEX [IX_OrderItems_OrderId] ON [dbo].[OrderItems]
(
	[OrderId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_OrderItems_ProductId]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE NONCLUSTERED INDEX [IX_OrderItems_ProductId] ON [dbo].[OrderItems]
(
	[ProductId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Orders_UserId]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE NONCLUSTERED INDEX [IX_Orders_UserId] ON [dbo].[Orders]
(
	[UserId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
/****** Object:  Index [IX_Products_CategoryId]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE NONCLUSTERED INDEX [IX_Products_CategoryId] ON [dbo].[Products]
(
	[CategoryId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_Email]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Email] ON [dbo].[Users]
(
	[Email] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
/****** Object:  Index [IX_Users_Username]    Script Date: 4/22/2025 1:04:58 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_Users_Username] ON [dbo].[Users]
(
	[Username] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (N'') FOR [Address]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (N'') FOR [City]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (N'') FOR [Country]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (N'') FOR [FullName]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (N'') FOR [PostalCode]
GO
ALTER TABLE [dbo].[Orders] ADD  DEFAULT (N'') FOR [State]
GO
ALTER TABLE [dbo].[Products] ADD  DEFAULT (CONVERT([bit],(0))) FOR [IsAvailable]
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [FK_CartItems_Carts_CartId] FOREIGN KEY([CartId])
REFERENCES [dbo].[Carts] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [FK_CartItems_Carts_CartId]
GO
ALTER TABLE [dbo].[CartItems]  WITH CHECK ADD  CONSTRAINT [FK_CartItems_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[CartItems] CHECK CONSTRAINT [FK_CartItems_Products_ProductId]
GO
ALTER TABLE [dbo].[Carts]  WITH CHECK ADD  CONSTRAINT [FK_Carts_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Carts] CHECK CONSTRAINT [FK_Carts_Users_UserId]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY([OrderId])
REFERENCES [dbo].[Orders] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Orders_OrderId]
GO
ALTER TABLE [dbo].[OrderItems]  WITH CHECK ADD  CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY([ProductId])
REFERENCES [dbo].[Products] ([Id])
GO
ALTER TABLE [dbo].[OrderItems] CHECK CONSTRAINT [FK_OrderItems_Products_ProductId]
GO
ALTER TABLE [dbo].[Orders]  WITH CHECK ADD  CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY([UserId])
REFERENCES [dbo].[Users] ([Id])
GO
ALTER TABLE [dbo].[Orders] CHECK CONSTRAINT [FK_Orders_Users_UserId]
GO
ALTER TABLE [dbo].[Products]  WITH CHECK ADD  CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY([CategoryId])
REFERENCES [dbo].[Categories] ([Id])
GO
ALTER TABLE [dbo].[Products] CHECK CONSTRAINT [FK_Products_Categories_CategoryId]
GO
USE [master]
GO
ALTER DATABASE [WatchStoreDB] SET  READ_WRITE 
GO
