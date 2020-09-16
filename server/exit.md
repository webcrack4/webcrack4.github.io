exit()函数定义在stdlib.h中，而_exit()定义在unistd.h中，
注：exit()就是退出，传入的参数是程序退出时的状态码，0表示正常退出，其他表示非正常退出，一般都用-1或者1，标准C里有EXIT_SUCCESS和EXIT_FAILURE两个宏，用exit(EXIT_SUCCESS);


_exit()函数的作用最为简单：直接使进程停止运行，清除其使用的内存空间，并销毁其在内核中的各种数据结构；exit() 函数则在这些基础上作了一些包装，在执行退出之前加了若干道工序。 

exit()函数与_exit()函数最大的区别就在于exit()函数在调用exit系统调用之前要检查文件的打开情况，把文件缓冲区中的内容写回文件，就是"清理I/O缓冲"。


### exit()在结束调用它的进程之前，要进行如下步骤： 

1.调用atexit()注册的函数（出口函数）；按ATEXIT注册时相反的顺序调用所有由它注册的函数,这使得我们可以指定在程序终止时执行自己的清理动作.例如,保存程序状态信息于某个文件,解开对共享数据库上的锁等.

2.cleanup()；关闭所有打开的流，这将导致写所有被缓冲的输出，删除用TMPFILE函数建立的所有临时文件.

3.最后调用_exit()函数终止进程。


### _exit做3件事（man）： 

1，Any  open file descriptors belonging to the process are closed 

2，any children of the process are inherited  by process 1, init 

3，the process's parent is sent a SIGCHLD signal

exit执行完清理工作后就调用_exit来终止进程。