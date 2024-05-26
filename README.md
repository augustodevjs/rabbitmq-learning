# Exemplos de integração com .NET e RabbitMQ

Este repositório contém uma coleção de exemplos de integração com RabbitMQ utilizando .NET. Cada exemplo demonstra um caso específico de uso do RabbitMQ, incluindo o uso de diferentes tipos de exchanges e filas.

## Configuração

### Pré-requisitos

Antes de executar qualquer exemplo, certifique-se de ter os seguintes componentes instalados:

- [.NET SDK](https://dotnet.microsoft.com/download)
- [Visual Studio](https://visualstudio.microsoft.com/)
- [Docker](https://www.docker.com/get-started)
- [RabbitMQ Docker Image](https://hub.docker.com/_/rabbitmq)

### Configurar RabbitMQ

1. **Clone o repositório**:

   ```sh
   https://github.com/augustodevjs/rabbitmq-learning
   ```

2. **Iniciar RabbitMQ com Docker**:

- Certifique-se de que o Docker está instalado e em execução.
- Execute o seguinte comando para iniciar uma instância do RabbitMQ usando Docker:

  ```sh
  docker run -d -it --name rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:3-management
  ```

- Acesse o management plugin do RabbitMQ em http://localhost:15672 (usuário: guest, senha: guest).

## Estrutura do Projeto

O repositório é organizado em pastas, cada uma contendo um exemplo específico:

### rabbitmq-example-basic

- **RConsumer**:
  - O RProducer envia mensagens para uma fila no RabbitMQ. Primeiro, ele se conecta ao RabbitMQ no localhost e declara uma fila chamada "queue-example-basic". Em seguida, ele envia mensagens "Hello World!" com um contador crescente para a fila a cada 200 milissegundos.
- **RProducer**:
  - O RConsumer recebe mensagens da mesma fila no RabbitMQ. Ele se conecta ao RabbitMQ no localhost e declara a mesma fila "queue-example-basic". Em seguida, ele fica esperando por mensagens e imprime cada mensagem recebida.

### round-robin-example

- **Consumidor**:
  - O Produtor envia mensagens para uma fila no RabbitMQ. Ele se conecta ao RabbitMQ no localhost, declara uma fila chamada "round-robin" e envia mensagens "OrderNumber: X" com um contador crescente para a fila a cada 2 segundos.
- **Produtor**:
  - O Consumidor recebe mensagens da fila "round-robin" no RabbitMQ. Ele se conecta ao RabbitMQ no localhost, declara a mesma fila e fica esperando por mensagens, imprimindo cada mensagem recebida.
- **Round Robin**:
  - O RabbitMQ usa o padrão round-robin para distribuir mensagens entre múltiplos consumidores de uma fila. Quando há vários consumidores escutando na mesma fila, o RabbitMQ distribui as mensagens de maneira uniforme entre eles, garantindo que cada consumidor receba aproximadamente o mesmo número de mensagens.

### api-rest-worker-rabbitmq

- **AppOrderWorker**:
  - O AppOrderWorker é um worker que consome mensagens da fila "orderQueue". Ele se conecta ao RabbitMQ no localhost, declara a mesma fila e fica esperando por mensagens. Quando uma mensagem é recebida, ela é desserializada de volta para um objeto de pedido e as informações do pedido são impressas no console. Se o processamento for bem-sucedido, o worker reconhece a mensagem. Se ocorrer um erro, a mensagem é reenviada para a fila.
- **WebAppOrder**:
  - O OrderController é um controlador web que recebe pedidos HTTP POST contendo informações de pedidos. Ele se conecta ao RabbitMQ no localhost, declara uma fila chamada "orderQueue", e envia os dados do pedido para essa fila em formato JSON.

### prefetch-rabbitmq

- **ConsumerPrefetch**:
  - O ConsumerPrefetch recebe mensagens da fila "order-prefetch" no RabbitMQ. Ele se conecta ao RabbitMQ no localhost, declara a mesma fila e cria dois workers (Worker A e Worker B) que consomem as mensagens. O método BasicQos é usado para limitar o número de mensagens que cada worker pode receber antes de confirmar o processamento (ack), garantindo o balanceamento de carga e evitando a sobrecarga dos consumidores.
- **ProdutorPrefetch**:
  - O ProdutorPrefetch envia mensagens para uma fila no RabbitMQ. Ele se conecta ao RabbitMQ no localhost, declara uma fila chamada "order-prefetch" e envia mensagens em lotes de 10, aguardando uma tecla ser pressionada para enviar cada novo lote.

### multiples-workers-and-channels

- **ConsumerMultiWorkerChannels**:
  - O ProdutorMultiWorkerChannels envia mensagens para uma fila no RabbitMQ utilizando dois canais. Ele se conecta ao RabbitMQ no localhost, declara uma fila chamada "order-multiple-channels" em cada canal, e envia mensagens "OrderNumber: X" a cada segundo de dois produtores diferentes (Produtor A e Produtor B).
- **ProdutorMultiWorkerChannels**:
  - O ConsumerMultiWorkerChannels recebe mensagens da fila "order-multiple-channels" no RabbitMQ. Ele se conecta ao RabbitMQ no localhost e cria dois canais. Para cada canal, ele cria sete trabalhadores (Worker 0 a Worker 6) que consomem as mensagens da fila, imprimindo cada mensagem recebida junto com o identificador do thread que a processou.

### exchange-direct-rabbitmq

- **ConsumidorExchangeDirect**:
  - O ProdutorExchangeDirect envia mensagens para diferentes filas usando uma exchange direta. Ele se conecta ao RabbitMQ no localhost, declara duas filas ("order" e "finance_orders") e uma exchange direta chamada "order". As filas são vinculadas à exchange com diferentes chaves de roteamento ("order_new" e "order_upd"). O produtor então envia mensagens de novas ordens e atualizações de ordens para a exchange, que roteia as mensagens para as filas apropriadas.
- **ProdutorExchangeDirect**:
  - O ConsumidorExchangeDirect recebe mensagens das filas "order" e "finance_orders". Ele se conecta ao RabbitMQ no localhost e cria seis trabalhadores: três para a fila "order" e três para a fila "finance_orders". Cada trabalhador consome mensagens da fila atribuída, processando e reconhecendo as mensagens. Se houver um erro, o trabalhador rejeita a mensagem.

### exchange-fanout-rabbitmq

- **ConsumidorExchangeFanout**:
  - O ProdutorExchangeFanout envia mensagens para todas as filas associadas a uma exchange do tipo "fanout". Ele se conecta ao RabbitMQ no localhost, declara três filas ("logs-exchange-fanout", "order-exchange-fanout" e "finance-orders_exchange-fanout") e uma exchange do tipo "fanout" chamada "order". Cada fila é vinculada à exchange "order". O produtor envia mensagens para a exchange, que distribui as mensagens para todas as filas vinculadas.
- **ProdutorExchangeFanout**:
  - O ConsumidorExchangeFanout recebe mensagens de uma das filas associadas à exchange "order". Ele se conecta ao RabbitMQ no localhost, declara a fila "finance-orders_exchange-fanout" (ou qualquer outra fila desejada) e cria dois trabalhadores que consomem mensagens dessa fila. Cada trabalhador processa e reconhece as mensagens recebidas. Se houver um erro, o trabalhador rejeita a mensagem.

### nack-autoack-rabbitmq

- **Consumidor-Ack-Nack-AutoAck**:
  - No Consumidor, a configuração inicial é feita para se conectar a um servidor RabbitMQ local (localhost) e declarar uma fila chamada "order-ack-nack-autoack". Em seguida, um consumidor é criado para escutar mensagens dessa fila. Quando uma mensagem é recebida, o código tenta processá-la e, se for bem-sucedido, envia uma confirmação de recebimento (ack) ao servidor RabbitMQ. Caso ocorra um erro durante o processamento, ele envia uma mensagem de não reconhecimento (nack), permitindo que a mensagem seja reencaminhada para o consumidor mais tarde. Este comportamento garante que mensagens problemáticas não sejam perdidas e possam ser reprocessadas.
- **Produtor-Ack-Nack-AutoAck**:
  - O programa do produtor, por sua vez, também se conecta ao mesmo servidor RabbitMQ e declara a mesma fila. Ele então entra em um loop infinito onde, a cada tecla pressionada pelo usuário, envia 100 mensagens para a fila. Cada mensagem contém um número de pedido incrementado e o nome do produtor ("Produtor A"). O programa utiliza uma thread separada para enviar as mensagens continuamente, até que ocorra algum erro. Se um erro ocorrer, ele exibe o erro e termina a execução.

### message-ttl-rabbitmq

- **MessageTTLRabbitMq**:

  - Este código configura uma aplicação simples para enviar uma mensagem a uma fila no RabbitMQ com um tempo de vida específico (TTL - Time To Live).

  - Primeiro, define-se o nome da fila como "test_time_to_live" e cria-se uma fábrica de conexões para se conectar ao RabbitMQ localizado no localhost. Com essa fábrica, o código estabelece uma conexão e cria um canal para a comunicação.

  - Em seguida, declara-se a fila com um argumento adicional que define o tempo de vida das mensagens (x-message-ttl) para 20.000 milissegundos, ou seja, 20 segundos. Isso significa que qualquer mensagem que não for consumida dentro desse período será automaticamente deletada pelo RabbitMQ.

  - Depois, o programa cria uma mensagem com o texto "Hello World!" e a data e hora atuais, e envia essa mensagem para a fila. Após enviar a mensagem, o programa aguarda que o usuário pressione [enter] para finalizar a execução.

  - O objetivo principal deste código é demonstrar como configurar uma fila no RabbitMQ para que as mensagens sejam automaticamente removidas se não forem processadas dentro de um tempo específico, garantindo assim que as mensagens antigas não permaneçam na fila indefinidamente.

### dead-letter-queue-rabbitmq

- **ConsumidorDeadLetterRabbitMq**:

  - Este código configura um consumidor RabbitMQ que usa uma fila de mensagens mortas (Dead Letter Queue) para mensagens que não podem ser processadas.

  - Primeiro, ele se conecta ao RabbitMQ no localhost e cria um canal de comunicação. Então, declara um "Exchange" chamado "DeadLetterExchange" e uma fila chamada "DeadLetterQueue", associando a fila ao exchange.

  - A seguir, cria uma fila principal chamada "task_queue", configurando-a para usar "DeadLetterExchange" como sua Dead Letter Exchange. Isso significa que mensagens não processadas serão enviadas para "DeadLetterQueue".

  - O consumidor é configurado para receber mensagens de "task_queue". Quando uma mensagem é recebida, ele tenta processá-la. Se o processamento for bem-sucedido, ele envia uma confirmação de recebimento (ack). Se ocorrer um erro, ele envia uma não confirmação (nack), movendo a mensagem para a Dead Letter Queue.

  - O programa fica aguardando mensagens e pode ser finalizado ao pressionar [enter].

### persistence-message-durable-queue-rabbitmq

- **PublishPersistenceMessage**:

  - Este código configura um produtor RabbitMQ que envia mensagens persistentes para garantir que não sejam perdidas mesmo que o servidor RabbitMQ reinicie.

  - Primeiro, ele se conecta ao RabbitMQ no localhost e cria um canal de comunicação. Em seguida, declara uma fila chamada "persistence-message" com a propriedade "durable" definida como verdadeira, o que significa que a fila persiste mesmo após reiniciar o servidor.

  - Depois, o programa define a mensagem "Hello world!" e a converte para um array de bytes.

  - Ele então cria propriedades básicas para a mensagem e define a propriedade "Persistent" como verdadeira, assegurando que a mensagem será salva em disco e não perdida em caso de falha do servidor.

  - Por fim, o programa publica a mensagem na fila "persistence-message" e exibe uma mensagem de confirmação no console. Ele aguarda que o usuário pressione [enter] para finalizar a execução.

  - Este código demonstra como configurar um produtor RabbitMQ para enviar mensagens que são persistentes, garantindo sua durabilidade.

### publish-confirmation-rabbitmq

- **PublishConfirmationRabbitMq**:

  - Esse código é um exemplo básico de como usar o RabbitMQ em C# para publicar mensagens e confirmar sua entrega. Ele cria uma conexão com o RabbitMQ, configura um canal de comunicação, declara uma fila chamada "order", publica uma mensagem nessa fila e espera por confirmações de que a mensagem foi entregue com sucesso.

  - O método Main é onde a execução começa. Ele configura a conexão com o RabbitMQ, cria um canal de comunicação e declara a fila "order". Em seguida, publica uma mensagem nessa fila usando channel.BasicPublish. Depois, espera por confirmações de entrega usando channel.WaitForConfirms.

  - Os métodos Channel_BasicAcks, Channel_BasicNacks e Channel_BasicReturn são chamados para lidar com confirmações de entrega ou falha. Se a mensagem for entregue com sucesso, Channel_BasicAcks é chamado. Se houver um problema na entrega, Channel_BasicNacks é chamado. E se a mensagem não puder ser entregue e for retornada, Channel_BasicReturn é chamado.

  - Por fim, o programa espera por uma entrada do usuário antes de encerrar.
