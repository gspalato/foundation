import React, { useEffect, useState } from 'react';
import { Navigate } from 'react-router-dom';

import Loading from '@app/Loading';

import FoundationClient from '@lib/api/client';
import { useAuth } from '@lib/auth';

const Component: React.FC<React.PropsWithChildren> = (props) => {
	const { token, logout } = useAuth();

	const [error, setError] = useState<string | null>();
	const [loading, setLoading] = useState(true);

	const check = async () => {
		if (!token) return;

		var response = await FoundationClient.CheckAuthentication(token);
		var data = await response.json();

		if (response.ok && data.successful) {
			setLoading(false);
		} else {
			setError(data.message);
			logout();
		}
	};

	useEffect(() => {
		check();
	}, [token]);

	if (loading) return <Loading />;

	if (error) {
		console.log(error);
		return <Navigate to='/login' />;
	}

	return token ? <>{props.children}</> : <Navigate to='/login' />;
};

Component.displayName = 'ProtectedRoute';

export default Component;
