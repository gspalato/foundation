import React, {
	createContext,
	PropsWithChildren,
	useContext,
	useEffect,
	useReducer,
	useState,
} from 'react';

import FoundationClient from '@lib/api/client';

import { AuthenticationPayload } from '@/types/AuthenticationPayload';
import { CheckAuthenticationPayload } from '@/types/CheckAuthenticationPayload';

export type AuthState = {
	token: string | null;
	isLoggedIn: boolean;
	isLoading: boolean;
};
export type AuthStateAction = { type: AuthStateActionType; [key: string]: any };
export type AuthStateActionType =
	| 'LOGIN'
	| 'LOGOUT'
	| 'SET_TOKEN'
	| 'RESTORE_TOKEN'
	| 'SET_LOGGED_IN'
	| 'SET_LOADING';

type AuthContextData = {
	login: (username: string, password: string) => Promise<boolean>;
	logout: () => void;
} & AuthState;

const AuthContext = createContext<AuthContextData>({} as any);
const useAuth = () => useContext(AuthContext);

const AuthProvider: React.FC<PropsWithChildren> = ({ children }) => {
	const [state, dispatch] = useReducer(
		(prevState: AuthState, action: AuthStateAction) => {
			switch (action.type) {
				case 'SET_LOADING':
					return {
						...prevState,
						isLoading: action.isLoading,
					};

				case 'SET_TOKEN':
					return {
						...prevState,
						token: action.token,
					};

				case 'RESTORE_TOKEN':
					return {
						...prevState,
						token: action.token,
						isLoading: false,
					};

				case 'SET_LOGGED_IN':
					return {
						...prevState,
						isLoggedIn: action.isLoggedIn,
						isLoading: false,
					};

				case 'LOGIN':
					return {
						...prevState,
						isLoggedIn: true,
						token: action.token,
						isLoading: false,
					};

				case 'LOGOUT':
					return {
						...prevState,
						isLoggedIn: false,
						isLoading: false,
						token: null,
					};
			}
		},
		{
			isLoading: false,
			isLoggedIn: false,
			token: null,
		}
	);

	useEffect(() => {
		// Fetch the token from storage then navigate to our appropriate place
		const bootstrapAsync = async () => {
			let token;

			try {
				token = sessionStorage.getItem('foundation-token');
			} catch (e) {
				// Restoring token failed
				return;
			}

			// After restoring token, we may need to validate it in production apps
			if (!token) return;

			try {
				const result = await FoundationClient.CheckAuthentication(
					token
				);

				const { successful }: CheckAuthenticationPayload =
					await result.json();

				if (!successful) return;

				// This will switch to the App screen or Auth screen and this loading
				// screen will be unmounted and thrown away.
				dispatch({ type: 'RESTORE_TOKEN', token: token });
				dispatch({ type: 'SET_LOGGED_IN', isLoggedIn: successful });
			} catch (e) {
				console.log(e);
			}
		};

		bootstrapAsync();
	}, []);

	const authContext = React.useMemo(
		() => ({
			...state,
			login: async (
				username: string,
				password: string
			): Promise<boolean> => {
				dispatch({ type: 'SET_LOADING', isLoading: true });

				const response = await FoundationClient.Authenticate(
					username,
					password
				);

				if (!response.ok) {
					alert(`Failed to login. Try again.`);
					console.log(response);
					return false;
				}

				const payload: AuthenticationPayload = await response.json();
				if (!payload.successful) {
					alert(`Failed to login. Try again.`);
					console.log(response);
					return false;
				}

				dispatch({ type: 'LOGIN', token: payload.token });

				return true;
			},
			logout: () => {
				sessionStorage.removeItem('foundation-token');

				dispatch({ type: 'LOGOUT' });
			},
		}),
		[state]
	);

	return (
		<AuthContext.Provider value={authContext}>
			{children}
		</AuthContext.Provider>
	);
};

AuthProvider.displayName = 'AuthProvider';

const useSetAuthToken = (token: string) => {
	useEffect(() => {
		const setToken = async () => {
			sessionStorage.setItem('foundation-token', token);
		};

		setToken();
	}, [token]);
};

const useAuthToken = (callback?: (token: string | null) => any) => {
	const [token, setToken] = useState<string | null>(null);
	useEffect(() => {
		const getToken = async () => {
			const token = sessionStorage.getItem('foundation-token');
			setToken(token);
			callback?.(token);
		};

		getToken();
	}, []);

	return token;
};

const useExpireAuthToken = (callback?: () => void) => {
	useEffect(() => {
		const setToken = async () => {
			sessionStorage.removeItem('foundation-token');
			callback?.();
		};

		setToken();
	}, [callback]);
};

export {
	AuthProvider,
	useAuth,
	useAuthToken,
	useExpireAuthToken,
	useSetAuthToken,
};
