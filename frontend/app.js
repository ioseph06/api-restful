// Configuración de la API
const API_BASE_URL = 'http://localhost:8080/api/v1'; // Ajusta según tu URL
let currentPage = 1;
const pageSize = 10;

// ==================== AUTENTICACIÓN ====================

// Login
document.getElementById('loginForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = document.getElementById('loginEmail').value;
    const password = document.getElementById('loginPassword').value;

    try {
        const response = await fetch(`${API_BASE_URL.replace('/v1', '')}/Auth/login`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Credenciales inválidas');
        }

        const data = await response.json();
        
        // Guardar token y datos del usuario
        localStorage.setItem('token', data.token);
        localStorage.setItem('userEmail', data.email);
        localStorage.setItem('userRoles', JSON.stringify(data.roles));

        showMainSection();
        loadProductos();
    } catch (error) {
        showError('loginError', error.message);
    }
});

// Registro
document.getElementById('registerForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    const email = document.getElementById('registerEmail').value;
    const password = document.getElementById('registerPassword').value;
    const confirmPassword = document.getElementById('registerConfirmPassword').value;

    if (password !== confirmPassword) {
        showError('registerError', 'Las contraseñas no coinciden');
        return;
    }

    try {
        const response = await fetch(`${API_BASE_URL.replace('/v1', '')}/Auth/register`, {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            body: JSON.stringify({ email, password, confirmPassword })
        });

        if (!response.ok) {
            const error = await response.json();
            const errors = Object.values(error.errors || {}).flat().join(', ');
            throw new Error(errors || 'Error al registrarse');
        }

        hideRegisterModal();
        showSuccess('registerSuccess', '¡Registro exitoso! Ahora puedes iniciar sesión.');
        setTimeout(() => hideRegisterModal(), 2000);
    } catch (error) {
        showError('registerError', error.message);
    }
});

// Logout
function logout() {
    localStorage.removeItem('token');
    localStorage.removeItem('userEmail');
    localStorage.removeItem('userRoles');
    document.getElementById('loginSection').classList.remove('hidden');
    document.getElementById('mainSection').classList.add('hidden');
}

// ==================== PRODUCTOS ====================

// Cargar productos con paginación
async function loadProductos(page = 1) {
    currentPage = page;
    const search = document.getElementById('searchInput').value;
    
    showLoading(true);
    
    try {
        const token = localStorage.getItem('token');
        const url = `${API_BASE_URL}/productos?pageNumber=${page}&pageSize=${pageSize}${search ? `&nombre=${search}` : ''}`;
            
        const response = await fetch(url, {
            headers: {
                'Authorization': `Bearer ${token}`,                
            }
        });

        if (response.status === 401) {
            logout();
            throw new Error('Sesión expirada');
        }

        if (!response.ok) throw new Error('Error al cargar productos');
        const data = await response.json();
                
        renderProductos(data.items);
        renderPagination(data);
    } catch (error) {
        showNotification(error.message, 'error');
    } finally {
        showLoading(false);
    }
}

// Crear producto
document.getElementById('createForm').addEventListener('submit', async (e) => {
    e.preventDefault();
    
    const nombre = document.getElementById('createNombre').value;
    const precio = parseFloat(document.getElementById('createPrecio').value);
    const enStock = document.getElementById('createEnStock').checked;

    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`${API_BASE_URL}/productos`, {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({ nombre, precio, enStock })
        });

        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Error al crear producto');
        }

        hideCreateModal();
        showNotification('Producto creado exitosamente', 'success');
        loadProductos(currentPage);
    } catch (error) {
        showError('createError', error.message);
    }
});

// Eliminar producto
async function deleteProducto(id) {
    if (!confirm('¿Estás seguro de eliminar este producto?')) return;

    try {
        const token = localStorage.getItem('token');
        const response = await fetch(`${API_BASE_URL}/productos/${id}`, {
            method: 'DELETE',
            headers: {
                'Authorization': `Bearer ${token}`
            }
        });

        if (!response.ok) throw new Error('Error al eliminar producto');

        showNotification('Producto eliminado', 'success');
        loadProductos(currentPage);
    } catch (error) {
        showNotification(error.message, 'error');
    }
}

// ==================== RENDERIZADO ====================

function renderProductos(productos) {
    const tbody = document.getElementById('productosTable');
    const emptyState = document.getElementById('emptyState');


       if (productos.length === 0) {
        tbody.innerHTML = '';
        emptyState.classList.remove('hidden');
        return;
    }

    

    emptyState.classList.add('hidden');
    tbody.innerHTML = productos.map(p => `
        <tr class="hover:bg-gray-50">
            <td class="px-6 py-4">${p.id}</td>
            <td class="px-6 py-4 font-medium">${p.nombre}</td>
            <td class="px-6 py-4">$${p.precio.toFixed(2)}</td>
            <td class="px-6 py-4">
                <span class="px-3 py-1 rounded-full text-sm ${p.enStock ? 'bg-green-100 text-green-800' : 'bg-red-100 text-red-800'}">
                    ${p.enStock ? 'En Stock' : 'Agotado'}
                </span>
            </td>
            <td class="px-6 py-4">
                <button onclick="deleteProducto(${p.id})" class="text-red-600 hover:text-red-800">
                    <i class="fas fa-trash"></i> Eliminar
                </button>
            </td>
        </tr>
    `).join('');
}

function renderPagination(data) {
    const pagination = document.getElementById('pagination');
console.log('Datos:', data); // Debug: Verificar datos de paginación    
console.log('Datos de paginación:', data.totalPages); // Debug: Verificar datos de paginación

    if (data.totalPages <= 1) {
        pagination.classList.add('hidden');
        return;
    }

    pagination.classList.remove('hidden');
    let html = '';

    // Botón anterior
    if (data.hasPreviousPage) {
        html += `<button onclick="loadProductos(${data.pageNumber - 1})" class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">Anterior</button>`;
    }

    // Información de página
    html += `<span class="px-4 py-2 text-gray-700">Página ${data.pageNumber} de ${data.totalPages}</span>`;

    // Botón siguiente
    if (data.hasNextPage) {
        html += `<button onclick="loadProductos(${data.pageNumber + 1})" class="px-4 py-2 bg-blue-600 text-white rounded-lg hover:bg-blue-700">Siguiente</button>`;
    }

    pagination.innerHTML = html;
}

// ==================== UTILIDADES ====================

function showMainSection() {
    document.getElementById('loginSection').classList.add('hidden');
    document.getElementById('mainSection').classList.remove('hidden');
    document.getElementById('userEmail').textContent = localStorage.getItem('userEmail');
    
    const roles = JSON.parse(localStorage.getItem('userRoles') || '[]');
    document.getElementById('userRoles').textContent = roles.length > 0 ? `(${roles.join(', ')})` : '';
}

function showCreateModal() {
    document.getElementById('createModal').classList.remove('hidden');
    document.getElementById('createModal').classList.add('flex');
    document.getElementById('createForm').reset();
    document.getElementById('createError').classList.add('hidden');
}

function hideCreateModal() {
    document.getElementById('createModal').classList.add('hidden');
    document.getElementById('createModal').classList.remove('flex');
}

function showRegisterModal() {
    document.getElementById('registerModal').classList.remove('hidden');
    document.getElementById('registerModal').classList.add('flex');
    document.getElementById('registerForm').reset();
    document.getElementById('registerError').classList.add('hidden');
    document.getElementById('registerSuccess').classList.add('hidden');
}

function hideRegisterModal() {
    document.getElementById('registerModal').classList.add('hidden');
    document.getElementById('registerModal').classList.remove('flex');
}

function showLoading(show) {
    document.getElementById('loadingIndicator').classList.toggle('hidden', !show);
    document.getElementById('productosTable').classList.toggle('hidden', show);
}

function showError(elementId, message) {
    const element = document.getElementById(elementId);
    element.textContent = message;
    element.classList.remove('hidden');
    setTimeout(() => element.classList.add('hidden'), 5000);
}

function showSuccess(elementId, message) {
    const element = document.getElementById(elementId);
    element.textContent = message;
    element.classList.remove('hidden');
}

function showNotification(message, type = 'success') {
    const notification = document.getElementById('notification');
    const messageEl = document.getElementById('notificationMessage');
    
    messageEl.textContent = message;
    notification.querySelector('div').className = `${type === 'success' ? 'bg-green-500' : 'bg-red-500'} text-white px-6 py-3 rounded-lg shadow-lg`;
    
    notification.classList.remove('hidden');
    setTimeout(() => notification.classList.add('hidden'), 3000);
}

// ==================== INICIALIZACIÓN ====================

// Verificar si ya hay una sesión activa
window.addEventListener('DOMContentLoaded', () => {
    const token = localStorage.getItem('token');
    if (token) {
        showMainSection();
        loadProductos();
    }
});