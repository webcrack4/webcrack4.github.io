1:UDP中可以使用connect系统调用


2:UDP中connect操作与TCP中connect操作有着本质区别。


TCP中调用connect会引起三次握手,client与server建立连结.UDP中调用connect内核仅仅把对端ip&port记录下来.


3:UDP中可以多次调用connect,TCP只能调用一次connect.  


UDP多次调用connect有两种用途:1,指定一个新的ip&port连结. 2,断开和之前的ip&port的连结.


指定新连结,直接设置connect第二个参数即可.


断开连结,需要将connect第二个参数中的sin_family设置成 AF_UNSPEC即可. 


4:UDP中使用connect可以提高效率.原因如下:


普通的UDP发送两个报文内核做了如下:#1:建立连结#2:发送报文#3:断开连结#4:建立连结#5:发送报文#6:断开连结


采用connect方式的UDP发送两个报文内核如下处理:#1:建立连结#2:发送报文#3:发送报文另外一点,  每次发送报文内核都由可能要做路由查询.