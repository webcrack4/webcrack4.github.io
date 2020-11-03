### 什么是死锁，如何避免死锁？

  线程A需要资源X，而线程B需要资源Y，而双方都掌握有对方所要的资源，这种情况称为死锁（deadlock），或死亡拥抱（the deadly embrace）。

在并发程序设计中，死锁 (deadlock) 是一种十分常见的逻辑错误。通过采用正确的编程方式，死锁的发生不难避免。

### 死锁的四个必要条件

在计算机专业的本科教材中，通常都会介绍死锁的四个必要条件。这四个条件缺一不可，或者说只要破坏了其中任何一个条件，死锁就不可能发生。我们来复习一下，这四个条件是：

1. 互斥（Mutual exclusion）：存在这样一种资源，它在某个时刻只能被分配给一个执行绪（也称为线程）使用；

2. 持有（Hold and wait）：当请求的资源已被占用从而导致执行绪阻塞时，资源占用者不但无需释放该资源，而且还可以继续请求更多资源；

3. 不可剥夺（No preemption）：执行绪获得到的互斥资源不可被强行剥夺，换句话说，只有资源占用者自己才能释放资源；

4. 环形等待（Circular wait）：若干执行绪以不同的次序获取互斥资源，从而形成环形等待的局面，想象在由多个执行绪组成的环形链中，每个执行绪都在等待下一个执行绪释放它持有的资源。

### 解除死锁的必要条件

不难看出，在死锁的四个必要条件中，第二、三和四项条件比较容易消除。通过引入事务机制，往往可以消除第二、三两项条件，方法是将所有上锁操作均作为事务对待，一旦开始上锁，即确保全部操作均可回退，同时通过锁管理器检测死锁，并剥夺资源（回退事务）。这种做法有时会造成较大开销，而且也需要对上锁模式进行较多改动。

消除第四项条件是比较容易且代价较低的办法。具体来说这种方法约定：上锁的顺序必须一致。具体来说，我们人为地给锁指定一种类似“水位”的方向性属性。无论已持有任何锁，该执行绪所有的上锁操作，必须按照一致的先后顺序从低到高（或从高到低）进行，且在一个系统中，只允许使用一种先后次序。

请注意，放锁的顺序并不会导致死锁。也就是说，尽管按照 锁A, 锁B, 放A, 放B 这样的顺序来进行锁操作看上去有些怪异，但是只要大家都按先A后B的顺序上锁，便不会导致死锁。
解决方法：


1 使用事务时，尽量缩短事务的逻辑处理过程，及早提交或回滚事务； (细化处理逻辑，执行一段逻辑后便回滚或者提交，然后再执行其它逻辑，直到事物执行完毕提交)

2 设置死锁超时参数为合理范围，如：3分钟-10分种；超过时间，自动放弃本次操作，避免进程悬挂； 

3 优化程序，检查并避免死锁现象出现； 

4 .对所有的脚本和SP都要仔细测试，在正是版本之前。 

5 所有的SP都要有错误处理（通过@error） 

6 一般不要修改SQL SERVER事务的默认级别。不推荐强行加锁

### 另外参考的解决方法：

按同一顺序访问对象
    如果所有并发事务按同一顺序访问对象，则发生死锁的可能性会降低。例如，如果两个并发事务获得 Supplier 表上的锁，然后获得 Part 表上的锁，则在其中一个事务完成之前，另一个事务被阻塞在 Supplier 表上。第一个事务提交或回滚后，第二个事务继续进行。不发生死锁。将存储过程用于所有的数据修改可以标准化访问对象的顺序。
    
    避免事务中的用户交互
    避免编写包含用户交互的事务，因为运行没有用户交互的批处理的速度要远远快于用户手动响应查询的速度，例如答复应用程序请求参数的提示。例如，如果事务正在等待用户输入，而用户去吃午餐了或者甚至回家过周末了，则用户将此事务挂起使之不能完成。这样将降低系统的吞吐量，因为事务持有的任何锁只有在事务提交或回滚时才会释放。即使不出现死锁的情况，访问同一资源的其它事务也会被阻塞，等待该事务完成。
    
    保持事务简短并在一个批处理中
    在同一数据库中并发执行多个需要长时间运行的事务时通常发生死锁。事务运行时间越长，其持有排它锁或更新锁的时间也就越长，从而堵塞了其它活动并可能导致死锁。保持事务在一个批处理中，可以最小化事务的网络通信往返量，减少完成事务可能的延迟并释放锁。
    
    使用低隔离级别
    确定事务是否能在更低的隔离级别上运行。执行提交读允许事务读取另一个事务已读取（未修改）的数据，而不必等待第一个事务完成。使用较低的隔离级别（例如提交读）而不使用较高的隔离级别（例如可串行读）可以缩短持有共享锁的时间，从而降低了锁定争夺。
    
    使用绑定连接
    使用绑定连接使同一应用程序所打开的两个或多个连接可以相互合作。次级连接所获得的任何锁可以象由主连接获得的锁那样持有，反之亦然，因此不会相互阻塞。