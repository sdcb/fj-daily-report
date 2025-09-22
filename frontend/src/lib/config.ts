// 在浏览器环境下，默认使用当前站点的 origin，便于静态导出后与后端同源部署
// 在服务端/构建阶段（例如 next dev 下的 Node 侧），回退到开发默认 5000 端口
export const getApiUrl = () => {
	const envUrl = process.env.NEXT_PUBLIC_API_URL;
	if (envUrl && envUrl.length > 0) return envUrl;
	if (typeof window !== 'undefined' && window.location && window.location.origin) {
		return window.location.origin;
	}
	return 'http://localhost:5000';
};