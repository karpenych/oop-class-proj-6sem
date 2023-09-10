<?php

$host = 'localhost';
$user = 'root';
$pass = 'usbw';
$dbname = 'bdforoop';

try {
    $DBH = new PDO("mysql:host=$host;dbname=$dbname", $user, $pass);
}
catch(PDOException $e) {
    echo $e->getMessage();
}

?>
