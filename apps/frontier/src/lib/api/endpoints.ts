const BaseUrl = 'https://foundation.unreal.sh/';

const getUrl = (...paths: string[]) => {
	let url = new URL(BaseUrl);
	paths.forEach((path) => (url = new URL(path, url)));
	return url.toString();
};

export const Endpoints = {
	GraphQL: {
		Gateway: getUrl('gql'),

		Identity: getUrl('identity', 'gql'),
		UPx: getUrl('identity', 'gql'),
	},

	REST: {
		Identity: getUrl('identity'),
		UPx: getUrl('upx'),
		RefillStationResumes: getUrl('upx', 'refillstation', 'resumes'),

		Auth: getUrl('identity', 'auth'),
		Avatar: getUrl('identity', 'me', 'avatar'),
	},
};

export default Endpoints;
