import 'file-loader?name=[name].[ext]!./index.html';
import React from "react";
import { createRoot } from 'react-dom/client';
import App from "./App";
import './App.scss';

import App02 from "./App02";
import './App02.scss';

const appElement = document.getElementById('main');
const root = createRoot(appElement);
root.render(<App02 />);
