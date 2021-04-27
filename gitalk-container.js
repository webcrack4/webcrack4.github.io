
/* 使用下面的 Javascript 代码生成 gitalk 插件 */
const gitalk = new Gitalk({
  clientID: '5fb4cba54ce83a85cd77', //GitHub Application Client ID

  clientSecret: 'f7ef1be8d0c080ce5b3e00bbf429b4c01566ac05 ', //GitHub Application Client Secret

  repo: 'myBlogTalk', //仓库名称(GitHub repo)
  owner: 'webcrack4', //仓库拥有者(GitHub repo owner)
  admin: ['webcrack4'],
  id: location.href,      // Ensure uniqueness and length less than 50
  distractionFreeMode: false  // Facebook-like distraction free mode
})

gitalk.render('gitalk-container')
