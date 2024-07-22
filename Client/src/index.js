import 'file-loader?name=[name].[ext]!./index.html';
import React from "react";
import { createRoot } from 'react-dom/client';
import App from "./App";
import './App.scss';

const appElement = document.getElementById('main');
const root = createRoot(appElement);
root.render(<App />);
