import React from 'react';
import { BrowserRouter } from 'react-router-dom';

import App from './App';

const Component = () => {
	return (
		<React.StrictMode>
			<BrowserRouter>
				<App />
			</BrowserRouter>
		</React.StrictMode>
	);
};

export default Component;
