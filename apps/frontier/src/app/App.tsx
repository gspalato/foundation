import { Route, Routes, useLocation } from 'react-router-dom';

import Dashboard from '@app/Dashboard';
import Home from '@app/Home';
import Login from '@app/Login';
import NotFound from '@app/NotFound';

import Content from '@components/Content';
import Cursor from '@components/Cursor';
import ProtectedRoute from '@components/ProtectedRoute';

import { AuthProvider } from '@lib/auth';

import { LayoutProvider } from '@/lib/layout';

import '@styles/globals.sass';

const App: React.FC = () => {
	const location = useLocation();

	return (
		<AuthProvider>
			<LayoutProvider>
				<Cursor />
				<Content>
					<Routes location={location} key={location.pathname}>
						<Route path='/' element={<Home />} />
						<Route
							path='/dashboard'
							element={
								<ProtectedRoute>
									<Dashboard />
								</ProtectedRoute>
							}
						/>
						<Route path='/login' element={<Login />} />
						<Route path='*' element={<NotFound />} />
					</Routes>
				</Content>
			</LayoutProvider>
		</AuthProvider>
	);
};

export default App;
