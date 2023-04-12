package main

import (
	"encoding/json"
	"fmt"
	"log"
	"net"
	"os"

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

type MoveData struct {
	Up    bool
	Right bool
	Down  bool
	Left  bool
	ID    int
}

type playerInfo struct {
	Connection net.Conn
	X          float32
	Z          float32
}

var clients map[int]playerInfo = make(map[int]playerInfo)

var queue deque.Deque[MoveData]

// ----------------------------------------------------------------------------
// ---------------------------Поток слушатель подключений----------------------
// ----------------------------------------------------------------------------

func main() {
	var firstPlayerConnInfo ToPlayer

	listen, err := net.Listen(_TYPE, _HOST+":"+_PORT) // Создаём слушатель подключений
	if err != nil {
		log.Fatal(err)
		os.Exit(1)
	}
	fmt.Printf("--------Сервер запущен--------\n\n")

	defer listen.Close()

	var closeConnections func() = func() {
		for _, playerInfo := range clients {
			playerInfo.Connection.Close()
		}
	}
	defer closeConnections()

	for {
		conn, err := listen.Accept() // Ждём подключения
		if err != nil {
			fmt.Println(err.Error())
			continue
		}
		fmt.Printf("----Клиент %s подключился\n", conn.RemoteAddr().String())

		data := make([]byte, 1024)
		read_len, err := conn.Read(data) // Принимаем id и координаты в JSON от клиента
		if err != nil {
			fmt.Println(err.Error())
			continue
		}
		fmt.Printf("----Приянтые данные: %v\n", string(data))

		data = data[:read_len]
		err = json.Unmarshal(data, &firstPlayerConnInfo) // Распаковка JSON
		if err != nil {
			fmt.Println(err.Error())
			continue
		}

		for _, playerInfo := range clients {
			_, err = playerInfo.Connection.Write([]byte(fmt.Sprintf("np%s", string(data)))) // Присылаем старым клиентам JSON инфу о новом игроке c префиксом "np"
			if err != nil {
				fmt.Printf("XXXXXX Не получилось отправить данные о новом игроке клиенту %s\n", conn.RemoteAddr().String())
				fmt.Println(err.Error())
				continue
			}
		}

		clients[firstPlayerConnInfo.ID] = playerInfo{ // Добавляем нового клиента в массив клиентов
			Connection: conn,
			X:          firstPlayerConnInfo.X,
			Z:          firstPlayerConnInfo.Z,
		}
		fmt.Printf("----Новый игрок: ID: %d; координаты%#v\n", firstPlayerConnInfo.ID, clients[firstPlayerConnInfo.ID])

		clients[firstPlayerConnInfo.ID].Connection.Write([]byte(fmt.Sprintf("%d", len(clients)))) // шлем новому клинту количество игроков в игре
		data = make([]byte, 1)
		read_len, err = conn.Read(data) // Принимаем подтверждение о получении
		if err != nil {
			fmt.Println("XXXXXX Не получилось принять подтверждение о получении количества челов")
			fmt.Println(err.Error())
			continue
		}
		fmt.Printf("----Клиент принял кол-во игроков: %s\n", string(data))

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

			clients[firstPlayerConnInfo.ID].Connection.Write(send_data) // отправляем чела новому игроку
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

		_player_ID := firstPlayerConnInfo.ID
		go MoveListener(_player_ID)
	}
}

// ----------------------------------------------------------------------------
// ---------------------------Поток слушатель подключений----------------------
// ----------------------------------------------------------------------------

func MoveListener(id int) {
	for {
		var playerMovement MoveData

		data := make([]byte, 1024)
		read_len, err := clients[id].Connection.Read(data) // Принимаем id и движение в JSON от клиента
		if err != nil {
			fmt.Println(err.Error())
			continue
		}

		data = data[:read_len]
		err = json.Unmarshal(data, &playerMovement) // Распаковка JSON
		if err != nil {
			fmt.Println(err.Error())
			continue
		}

		queue.PushBack(playerMovement)
		fmt.Printf("****Встало в очередь: %s --- %d\n", string(data), queue.Len())
	}
}
