import { createBrowserRouter, RouterProvider } from 'react-router'
import { AuthProvider } from './auth/AuthProvider'
import { RequireAuth } from './auth/guards'
import { Layout } from './components/Layout'
import { HomePage } from './pages/HomePage'
import { SignInPage } from './pages/SignInPage'

const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      { path: 'sign-in', element: <SignInPage /> },
      {
        // Everything else needs a signed-in user. Role-only pages also wrap themselves in <RequireRole>.
        element: <RequireAuth />,
        children: [{ index: true, element: <HomePage /> }],
      },
    ],
  },
])

export default function App() {
  return (
    <AuthProvider>
      <RouterProvider router={router} />
    </AuthProvider>
  )
}
