import { createBrowserRouter, RouterProvider } from 'react-router'
import { Layout } from './components/Layout'
import { HomePage } from './pages/HomePage'

const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [{ index: true, element: <HomePage /> }],
  },
])

export default function App() {
  return <RouterProvider router={router} />
}
