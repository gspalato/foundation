import Endpoints from '@lib/api/endpoints';

export class FoundationClient {
	public static Authenticate(
		username: string,
		password: string
	): Promise<Response> {
		return fetch(Endpoints.REST.Auth, {
			method: 'POST',
			headers: {
				'Content-Type': 'application/json',
			},
			body: JSON.stringify({ username, password }),
		});
	}

	public static CheckAuthentication(token: string): Promise<Response> {
		return fetch(Endpoints.REST.Auth, {
			method: 'GET',
			headers: {
				Authorization: `Bearer ${token}`,
			},
		});
	}

	public static UploadAvatar(
		token: string,
		image: { uri: string; name?: string; type?: string }
	): Promise<Response> {
		const formData = new FormData();
		formData.append('file', image as any);

		return fetch(Endpoints.REST.Avatar, {
			method: 'PUT',
			headers: {
				Authorization: `Bearer ${token}`,
			},
			body: formData,
		});
	}

	public static DeleteAvatar(token: string): Promise<Response> {
		return fetch(Endpoints.REST.Avatar, {
			method: 'DELETE',
			headers: {
				Authorization: `Bearer ${token}`,
			},
		});
	}

	public static GetRefillStationResumes(): Promise<Response> {
		return fetch(Endpoints.REST.RefillStationResumes, {
			method: 'GET',
		});
	}
}

export default FoundationClient;
