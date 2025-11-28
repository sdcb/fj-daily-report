// 获取 API 基础 URL
// 生产环境：前端静态文件与后端同源部署，返回空字符串（相对路径）
// 开发环境：通过环境变量 NEXT_PUBLIC_API_URL 指定后端地址
export const getApiUrl = (): string => {
	// 优先使用环境变量
	const envUrl = process.env.NEXT_PUBLIC_API_URL;
	if (envUrl && envUrl.length > 0) return envUrl;
	
	// 浏览器端：检查 window.API_URL（可通过脚本注入）
	if (typeof window !== 'undefined') {
		const windowApiUrl = (window as any)['API_URL'];
		if (windowApiUrl) return windowApiUrl;
		// 同源部署时返回空字符串
		return '';
	}
	
	// 服务端渲染时的回退值
	return 'http://localhost:5000';
};