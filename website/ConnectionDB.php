<?php
    $host = "mysql-3ba0c1be-quanlysukien2026.h.aivencloud.com";
    $port = "19299";
    $dbname = "quan_ly_su_kien"; 
    $username = "avnadmin";
    $password = "";

    $dsn = "mysql:host=$host;port=$port;dbname=$dbname;charset=utf8mb4";

    try {
        $conn = new PDO($dsn, $username, $password, [
            PDO::ATTR_ERRMODE => PDO::ERRMODE_EXCEPTION,
            PDO::MYSQL_ATTR_SSL_VERIFY_SERVER_CERT => false, 
        ]);
        
        echo "Kết nối Aiven thành công!";
    } 
    catch(PDOException $e) {
        echo "Kết nối không thành công: " . $e->getMessage();
    }
?>