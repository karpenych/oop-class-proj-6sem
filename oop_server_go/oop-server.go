package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net"
	"os"
	"os/signal"
	"strings"
	"syscall"

	"github.com/gammazero/deque"
)

const (
	_HOST = "localhost"
	_PORT = "8080"
	_TYPE = "tcp"
)

type ToPlayer struct {
	ID int
	X  float32
	Z  float32
}

type MoveDataFrom struct {
	Up    bool
	Right bool
	Down  bool
	Left  bool
	ID    int
	X     float32
	Z     float32
}

type MoveDataTo struct {
	Up    bool
	Right bool
	Down  bool
	Left  bool
	ID    int
}

type playerInfo struct {
	Connection      net.Conn
	GameHandlerConn net.Conn
	X               float32
	Z               float32
}

var clients map[int]playerInfo = make(map[int]playerInfo)

var queue deque.Deque[MoveDataTo]

// ----------------------------------------------------------------------------
// ---------------------------Поток слушатель подключений----------------------
// ----------------------------------------------------------------------------

func main() {
	go GracefulShutdown()

	var firstPlayerConnInfo ToPlayer

	listen, err := net.Listen(_TYPE, _HOST+":"+_PORT) // Создаём слушатель подключений
	if err != nil {
		log.Fatal(err)
		os.Exit(1)
	}
	fmt.Printf("\n--------Сервер запущен--------\n\n")

	defer listen.Close() //Выключаем листенер после выключения сервера

	go RunServerTools() //Запускаем поток управление сервера
	go MoveSender()     //Запускаем поток отправитель новых координат

	for {
		conn, err := listen.Accept() // Ждём подключения
		if err != nil {
			fmt.Printf("XXXХХ Не получилось принять подключение\n")
			fmt.Println(err.Error())
			continue
		}
		fmt.Printf("----Клиент %s подключился\n", conn.RemoteAddr().String())

		_gameHendlerConn, err := listen.Accept() // Ждём подключения
		if err != nil {
			fmt.Printf("XXXХХ Не получилось принять подключение 2\n")
			fmt.Println(err.Error())
			continue
		}
		fmt.Printf("----Соединение GameHandler %s подключено\n", _gameHendlerConn.RemoteAddr().String())

		data := make([]byte, 1024)
		read_len, err := conn.Read(data) // Принимаем id и координаты в JSON от нового клиента
		if err != nil {
			fmt.Printf("XXXХХ Не получилось принять инфу от нового клиента\n")
			fmt.Println(err.Error())
			continue
		}
		fmt.Printf("----Приянтые данные: %v\n", string(data))

		data = data[:read_len]
		err = json.Unmarshal(data, &firstPlayerConnInfo) // Распаковка JSON
		if err != nil {
			fmt.Printf("XXXХХ Не получилось размаршалить инфу нового клиента\n")
			fmt.Println(err.Error())
			continue
		}

		for _, playerInfo := range clients { // Присылаем старым клиентам JSON инфу о новом игроке c префиксом "np"
			_, err = playerInfo.GameHandlerConn.Write([]byte(fmt.Sprintf("np%s", string(data))))
			if err != nil {
				fmt.Printf("XXXXXX Не получилось отправить данные о новом игроке клиенту %s\n", conn.RemoteAddr().String())
				fmt.Println(err.Error())
				continue
			}
		}

		clients[firstPlayerConnInfo.ID] = playerInfo{ // Добавляем нового клиента в массив клиентов
			Connection:      conn,
			GameHandlerConn: _gameHendlerConn,
			X:               firstPlayerConnInfo.X,
			Z:               firstPlayerConnInfo.Z,
		}
		fmt.Printf("----Новый игрок: ID: %d; инфа: %#v\n", firstPlayerConnInfo.ID, clients[firstPlayerConnInfo.ID])

		// шлем новому клинту количество игроков в игре
		_, err = clients[firstPlayerConnInfo.ID].Connection.Write([]byte(fmt.Sprintf("%d", len(clients))))
		if err != nil {
			fmt.Println("XXXXXX Не получилось отправить кол-во игроков новому клиенту")
			fmt.Println(err.Error())
			continue
		}

		data = make([]byte, 1)
		read_len, err = conn.Read(data) // Принимаем подтверждение о получении
		if err != nil {
			fmt.Println("XXXXXX Не получилось принять подтверждение о получении количества челов")
			fmt.Println(err.Error())
			continue
		}
		fmt.Printf("----Клиент принял кол-во игроков (%d): %s\n", len(clients), string(data))

		for _id, _playerInfo := range clients { // отправляем всех игроков (по одному) новому игроку
			to_player := ToPlayer{
				ID: _id,
				X:  _playerInfo.X,
				Z:  _playerInfo.Z,
			}

			send_data, err := json.Marshal(to_player) // маршализируем инфу о челе
			if err != nil {
				fmt.Println("XXXXXX Не получилось замаршалить инфу о игроке")
				fmt.Println(err.Error())
				continue
			}

			_, err = clients[firstPlayerConnInfo.ID].Connection.Write(send_data) // отправляем чела новому игроку
			if err != nil {
				fmt.Println("XXXXXX Не получилось отправили игрока новому клиенту")
				fmt.Println(err.Error())
				continue
			}
			fmt.Printf("++++Отправили игрока: %s\n", string(send_data))

			data := make([]byte, 1)
			_, err = conn.Read(data) // Принимаем подтверждение о получении
			if err != nil {
				fmt.Println("XXXXXX Не получилось принять подтверждение о получении инфы о челе")
				fmt.Println(err.Error())
				continue
			}
			fmt.Printf("----Клиент принял инфу о игроке: %s\n", string(data))
		}
		fmt.Println()
		_player_ID := firstPlayerConnInfo.ID
		go MoveListener(_player_ID) //Принемаем движения игрока
	}
}

func GracefulShutdown() {
	sigChan := make(chan os.Signal, 1)
	signal.Notify(sigChan, syscall.SIGINT, syscall.SIGTERM)
	select {
	case sig := <-sigChan:
		for _id, playerInfo := range clients {
			_, err := playerInfo.GameHandlerConn.Write([]byte(fmt.Sprintf("servend"))) //Отправить всем сигнал о выключении сервера
			if err != nil {
				fmt.Printf("XXXXXX Не получилось отправить игроку %d данные о завершении сервера\n", _id)
				fmt.Println(err.Error())
				continue
			}
			playerInfo.Connection.Close()
			playerInfo.GameHandlerConn.Close()
		}
		fmt.Println("Сервер остановлен сигналом", sig)
		os.Exit(1)
	}
}

// ----------------------------------------------------------------------------
// ---------------------------Поток слушатель движения----------------------
// ----------------------------------------------------------------------------

func MoveListener(id int) {
	for {
		defer func() {
			if err := recover(); err != nil {
				log.Println("ИГРОК ОТКЛЮЧЁН:", err)
			}
		}()

		var playerMovementFrom MoveDataFrom
		var playerMovementTo MoveDataTo

		data := make([]byte, 1024)

		read_len, err := clients[id].Connection.Read(data) // Принимаем id и движение в JSON от клиента
		if err != nil {
			fmt.Printf("XXXХХ Не получилось принять движение\n")
			fmt.Println(err.Error())
			continue
		}

		data = data[:read_len]

		if string(data) == "end" { //Если пришёл сигнал end то закрываем подключение
			fmt.Printf("::::Клиент %d (%#v) отключился\n", id, clients[id])
			break
		}

		err = json.Unmarshal(data, &playerMovementFrom) // Распаковка JSON
		if err != nil {
			fmt.Printf("XXXХХ Не получилось размаршалить движение\n")
			fmt.Println(err.Error())
			continue
		}

		client := clients[playerMovementFrom.ID]
		client.X = playerMovementFrom.X
		client.Z = playerMovementFrom.Z
		clients[playerMovementFrom.ID] = client

		playerMovementTo.Up = playerMovementFrom.Up
		playerMovementTo.Right = playerMovementFrom.Right
		playerMovementTo.Down = playerMovementFrom.Down
		playerMovementTo.Left = playerMovementFrom.Left
		playerMovementTo.ID = playerMovementFrom.ID

		queue.PushBack(playerMovementTo) //Записываем движение в очередь

		//fmt.Printf("****Новые координаты X: %#v; Z: %#v; у %d\n", clients[playerMovementFrom.ID].X, clients[playerMovementFrom.ID].Z, playerMovementFrom.ID)
		//fmt.Printf("****Встало в очередь (*без координат): %s  (%d)\n", string(data), queue.Len())
	}

	if _, ok := clients[id]; !ok {
		clients[id].Connection.Close()
		clients[id].GameHandlerConn.Close()
		delete(clients, id)
		go DeletePlayer(id)
	}
}

// ----------------------------------------------------------------------------
// ---------------------------Поток отправитель движения----------------------
// ----------------------------------------------------------------------------

func MoveSender() {
	var playerMovementTo MoveDataTo

	for {
		if queue.Len() == 0 { //Если нет движения то ждём
			continue
		}

		playerMovementTo = queue.PopFront() //Берем движение из очереди

		send_movement, err := json.Marshal(playerMovementTo) // маршализируем движение
		if err != nil {
			fmt.Println("XXXXXX Не получилось замаршалить перемещение игрока")
			fmt.Println(err.Error())
			continue
		}

		for _, player := range clients { //Отправляем всем движение из очереди
			_, err := player.GameHandlerConn.Write(send_movement)
			if err != nil {
				fmt.Printf("XXXXXX Не получилось отправить данные о перемещении игрока\n")
				fmt.Println(err.Error())
				continue
			}
		}
		//fmt.Printf("****Ушло всем из очереди: %#v\n", playerMovementTo)
	}
}

// ----------------------------------------------------------------------------
// ----------------------------Поток отключения клиента------------------------
// ----------------------------------------------------------------------------

func DeletePlayer(id int) {
	for _, player := range clients { //Отправляем всем что игрок отключился
		_, err := player.GameHandlerConn.Write([]byte(fmt.Sprintf("dp%d", id)))
		if err != nil {
			fmt.Printf("XXXXXX Не получилось отправить данные об отключении игрока\n")
			fmt.Println(err.Error())
			continue
		}
	}
}

// ----------------------------------------------------------------------------
// ----------------------------Поток управление сервера------------------------
// ----------------------------------------------------------------------------

func RunServerTools() {
	fmt.Printf(".....введите help для просмотра комманд.....\n\n")
	var command string
	for {
		fmt.Scanf("%s\n", &command)

		if strings.ToLower(command) == "help" { //---------------------help
			fmt.Printf("\n$showplayers - показать всех игроков в сети\n")
			fmt.Println("$del - удалить игрока")
			fmt.Printf("$exit - выключить сервер\n\n")
		}

		if strings.ToLower(command) == "showplayers" { //--------showplayers
			fmt.Printf("----Вывод всех игроков в сети:\n")
			for key, value := range clients {
				fmt.Printf("--ID - %d; Info - %#v:\n", key, value)
			}
			fmt.Println()
		}

		if strings.ToLower(command) == "del" { //----------------------del
			var _id int
			fmt.Printf("введите ID игрока: ")
			fmt.Scanf("%d\n", &_id)

			_, err := clients[_id].GameHandlerConn.Write([]byte(fmt.Sprintf("servend"))) //Отправить игроку сигнал о выключении
			if err != nil {
				fmt.Printf("XXXXXX Не получилось отправить игроку %d данные о завершении\n", _id)
				fmt.Println(err.Error())
				continue
			}

			clients[_id].Connection.Close()
			clients[_id].GameHandlerConn.Close()
			delete(clients, _id)
			go DeletePlayer(_id)
		}

		if strings.ToLower(command) == "exit" { //----------------------exit
			for _id, playerInfo := range clients {
				_, err := playerInfo.GameHandlerConn.Write([]byte(fmt.Sprintf("servend"))) //Отправить всем сигнал о выключении сервера
				if err != nil {
					fmt.Printf("XXXXXX Не получилось отправить игроку %d данные о завершении сервера\n", _id)
					fmt.Println(err.Error())
					continue
				}
				playerInfo.Connection.Close()
				playerInfo.GameHandlerConn.Close()
			}
			fmt.Println("Сервер остановлен")
			os.Exit(1)
		}
	}
}
