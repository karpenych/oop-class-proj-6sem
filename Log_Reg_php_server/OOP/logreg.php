<?php

include "db.php";


$type = $_POST['type'];


$player = array(
    "id" =>0,
    "pos" => "Null"
);

$error = array(
    "errorText"=>"empty",
    "isError" => false
);

$userData = array(
    "playerData" => $player,
    "error" => $error
);


if($type == "logging"){#-----------------------------------------------LOGGINING-------------------------------------
    $login    = $_POST['login'];
    $password = $_POST['password'];
    if(isset($login) && isset($password)){
        $users = $DBH->query("SELECT * FROM `users` WHERE `login` = '$login'");
        if($users->rowCount() == 1){
            $user = $users->fetch(PDO::FETCH_ASSOC);
            if(password_verify($password, $user['password'])){
                $data = $DBH->query("SELECT * FROM `user_data` WHERE `user_id` = {$user['id']}")->fetch(PDO::FETCH_ASSOC);
                $userData["playerData"]["id"] = $user['id'];
                $userData["playerData"]["pos"] = $data['user_pos'];
            }else{
                SetError("Password are incorrect");
            }
        }else{
            SetError("Such User isn't exist");
        }
    }
}else if($type == "register"){#-----------------------------------------REGISTRATION-------------------------------------
    $login        = $_POST['login'];
    $password     = $_POST['password'];
    $confirm_pass = $_POST['confirmPassword'];
    if(isset($login) && isset($password) && isset($confirm_pass)){
        $users = $DBH->query("SELECT * FROM `users` WHERE `login` = '$login'");
        if($users->rowCount() == 0){
            if($password == $confirm_pass){
                $hash = password_hash($password, PASSWORD_DEFAULT);
                $DBH->query("INSERT INTO `users`(`login`, `password`) VALUES ('$login', '$hash')");
                $DBH->query("INSERT INTO `user_data`(`user_id`, `user_pos`) VALUES ({$DBH->lastInsertId()},'Null')");
            }else{
                SetError("Passwords dont't much");
            }
        }else{
            SetError("User Exist");
        }
    }
}else if($type == "save"){#----------------------------------------------DATA_SAVING---------------------------------------
    if(isset($_POST['id']) && isset($_POST['pos'])){
        $DBH->query("UPDATE `user_data` SET `user_pos`='{$_POST['pos']}' WHERE `user_id` = {$_POST['id']}");
    }
}else{
    SetError("Unknown data");
}


function SetError($text){ # NULL_DATA and ERROR_MESSAGE
    global $userData;

    $userDara["playerData"] = null;
    $userData["error"]["isError"] = true;
    $userData["error"]["errorText"] = "Error: ".$text;
}


echo json_encode($userData, JSON_UNESCAPED_UNICODE); # USER_DATA -> JSON

?>
