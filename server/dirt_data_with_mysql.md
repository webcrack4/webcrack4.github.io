CAS(Compare And Set)来降低读写锁冲突。常见的版本通过版本比较来实现乐观锁，当然也很简单新增version 字段来保证自己本次操作是独立的，操作完并更新version得到new_version，当并发时候别人拿到之前version则操作失败

当发生并发时候，Client-1、Client-2、Client-3，获取到的 goods_number 都为10，version 都为 0，当Client -1~3任意一个用户操作，则version 将会增加1，这样一来其他人就会更新失败 。