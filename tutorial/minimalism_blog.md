## github pages简易博客搭建教程

### 为什么需要博客？

	博客是一个给人沉淀知识和复习的空间，给自己一个与自己对话的空间，一种基于分享主义给别人分享带来的连锁反应。

## 为什么需要github pages和简易博客搭建

### 博客搭建要考虑什么？

	1.长期使用需要平台大，平台大才会稳定，长期看不会关闭的，长期收费低或者免费
	
	2.流量空间充足
	
	3.无广告、干净美观
	
	4.长期来看不易被墙或者被封、没有内容审查、内容合法性
	
	5.简易方便，手机电脑都可以编辑

	6.可以分享

	7.方便迁移
	

基于以上标准，那么可以找出不合需求的

	csdn、博客园（广告太多、主题单一）
	
	163博客(已经倒闭)、QQ空间、微信公众号(不稳，也有倒闭可能)
	
	自己租赁vps搭建博客、印象笔记、码云gitee（流量空间极其有限）
	
	国内的一切博客平台（内容审查）

	LOFTER、简书（难以迁移）
	
	WordPress、wix（长期收费高）
	
	Github Pages+Hexo(需要有机器编译，手机无法操作）
	
	blogger、汤不热（被墙了、无法访问、无法分享）

从长期来看，github pages上虽然也有很多国内不合法的内容，但是由于其技术上的不可取代性，曾经也被墙过多次但最终还是解除了墙，没有内容审查压力。其次平台够大（微软是老板），纵观博客流行开始十多年，能够维持大用户流量的博客平台，基本可以认为不会倒闭。还有收费是免费的、空间流量不限、干净美观无广告。那么github pages写静态博客页面是目前需求下写博客的最佳方案。

### 到底怎么用github pages可以操作简单，手机电脑都方便操作

其实方法就是github pages搭好直接写，不要编译那一套，网上使用github pages教程的方法几乎都是走编译那一套

第一步：开好账号注册并设置[三分钟在GitHub上搭建个人博客](https://zhuanlan.zhihu.com/p/28321740)

第二步：手机和电脑都装git客户端，手机要可以支持管理文件夹和编辑Markdown文档的，也可以直接浏览器打开

第三步: 仓库代码拉下来，修改_config.yml，后面直接加下面代码，改完push回去

```markdown
plugins:
  - jekyll-relative-links
relative_links:
  enabled: true
  collections: true
```
  
这样就可以支持相对路径跳转页面了

然后就可以愉快地写博客了，只需要创建一个Markdown文件，把跳转页面加好，push回去大概等个5分钟左右（github pages后台会自动用jekyll套件生成页面）,就可以看到你新博客内容了

<link rel="stylesheet" href="https://unpkg.com/gitalk/dist/gitalk.css">
<script src="https://unpkg.com/gitalk/dist/gitalk.min.js"></script>
<!-- 在页面中添加一个容器-->
<div id="gitalk-container"></div>
<script src="./../gitalk-container.js"></script>

