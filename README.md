# SitemapIndexNowApp

## Overview
SitemapIndexNowApp is a C# application that retrieves sitemaps from a MySQL database and submits them to search engines via the IndexNow protocol. This helps search engines efficiently update indexed content for better visibility.

## Database Initialization
To set up the MySQL database, execute the following SQL commands:

```sql
CREATE DATABASE examplecomdb_sitemap;

DROP TABLE IF EXISTS `tbindexnow`;
CREATE TABLE `tbindexnow` (
  `indexnowid` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `sitemapid` BIGINT(20) DEFAULT NULL,
  `lastupdatedate` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP(),
  PRIMARY KEY (`indexnowid`)
) ENGINE=InnoDB AUTO_INCREMENT=2 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

INSERT INTO `tbindexnow` VALUES (1, 0, '2025-02-12 14:35:04');

DROP TABLE IF EXISTS `tbsitemap`;
CREATE TABLE `tbsitemap` (
  `sitemapid` BIGINT(20) NOT NULL AUTO_INCREMENT,
  `creationdate` TIMESTAMP NULL DEFAULT CURRENT_TIMESTAMP(),
  `loc` VARCHAR(2000) NOT NULL,
  `lastmod` VARCHAR(100) DEFAULT NULL,
  `changefreq` VARCHAR(50) DEFAULT NULL,
  `priority` DOUBLE DEFAULT 0.5,
  `sitemapname` VARCHAR(100) DEFAULT NULL,
  `relatedproductid` VARCHAR(100) DEFAULT '0',
  PRIMARY KEY (`sitemapid`)
) ENGINE=InnoDB AUTO_INCREMENT=5 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_uca1400_ai_ci;

INSERT INTO `tbsitemap` VALUES
(1, '2025-02-12 14:42:23', 'https://www.example.com/', '2025-02-12T09:04:30+03:00', 'hourly', 0.5, 'sitemap_1.xml', '0'),
(2, '2025-02-12 14:42:23', 'https://www.example.com/url1', '2025-02-12T09:04:30+03:00', 'monthly', 0.5, 'sitemap_1.xml', '0'),
(3, '2025-02-12 14:42:23', 'https://www.example.com/url2', '2025-02-12T09:04:30+03:00', 'monthly', 0.5, 'sitemap_1.xml', '0'),
(4, '2025-02-12 14:42:23', 'https://www.example.com/url2', '2025-02-12T09:04:30+03:00', 'monthly', 0.5, 'sitemap_2.xml', '0');
```

## Configuration Setup
The application reads configuration settings from a JSON file. Modify the parameters based on your domain and setup:

**example.com.json**
```json
{
  "ConnectionString": "Server=localhost;Database=examplecomdb_sitemap;User Id=username;Password=password;",
  "PageSize": 1000,
  "IndexNowId": 1,
  "Host": "www.example.com",
  "ApiKey": "24c4d7a83f1d0e7617511360e761751136",
  "KeyLocation": "https://www.example.com/24c4d7a83f1d0e7617511360e761751136.txt",
  "SleepTime": 10000,
  "IndexNowEndpoints": {
    "Yandex": "https://www.yandex.com/IndexNow",
    "IndexNow": "https://api.indexnow.org/IndexNow",
    "Bing": "https://www.bing.com/indexnow"
  }
}
```

### Configuration Parameters
- **ConnectionString**: Database connection settings.
- **PageSize**: Number of records to process per request.
- **IndexNowId**: Identifier for the indexing process.
- **Host**: Website domain.
- **ApiKey**: Unique key for IndexNow authentication.
- **KeyLocation**: URL where the IndexNow key is stored.
- **SleepTime**: Delay in milliseconds between requests (10,000 ms = 10 seconds).
- **IndexNowEndpoints**: API endpoints for Yandex, IndexNow, and Bing submissions.

## Running the Application
Run the application using the following command:

```sh
SitemapIndexNowApp.exe example.com.json
```

This will process sitemaps from the database and submit them to the search engines using the configured IndexNow endpoints.

