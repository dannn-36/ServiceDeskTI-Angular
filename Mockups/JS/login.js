// Usuarios de ejemplo
const users = [
    { email: 'carlos.mendoza@gmail.com', password: '123456', name: 'Carlos Mendoza', department: 'Ventas' },
    { email: 'maria.gonzalez@gmail.com', password: '123456', name: 'María González', department: 'Contabilidad' },
    { email: 'roberto.silva@gmail.com', password: '123456', name: 'Roberto Silva', department: 'Marketing' },
    { email: 'ana.lopez@gmail.com', password: 'admin123', name: 'Ana López', role: 'Agente TI Senior' },
    { email: 'juan.perez@gmail.com', password: 'admin123', name: 'Juan Pérez', role: 'Agente TI' },
    { email: 'sofia.martinez@gmail.com', password: 'admin123', name: 'Sofía Martínez', role: 'Supervisora TI' }
];

function togglePassword() {
    const passwordInput = document.getElementById('password');
    const passwordToggle = document.getElementById('passwordToggle');
    
    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        passwordToggle.textContent = '🙈';
    } else {
        passwordInput.type = 'password';
        passwordToggle.textContent = '👁️';
    }
}

function handleLogin(event) {
    event.preventDefault();

    const formData = new FormData(event.target);
    const email = formData.get('email');
    const password = formData.get('password');

    // Validar credenciales
    const user = users.find(u => u.email === email && u.password === password);

    if (user) {
        showNotification('¡Inicio de sesión exitoso! Redirigiendo...', 'success');
        setTimeout(() => {
            showNotification('Redirigiendo...', 'info');
            // Aquí redirigiría según el tipo de usuario si lo deseas
            console.log('Redirigir a la vista correspondiente');
        }, 1500);
    } else {
        showNotification('Credenciales incorrectas. Verifica tu email y contraseña.', 'error');
        // Efecto de shake en el formulario
        const form = document.getElementById('loginForm');
        form.style.animation = 'shake 0.5s ease-in-out';
        setTimeout(() => {
            form.style.animation = '';
        }, 500);
    }
}

function showForgotPassword() {
    document.getElementById('forgotPasswordModal').classList.remove('hidden');
}

function closeForgotPassword() {
    document.getElementById('forgotPasswordModal').classList.add('hidden');
}

function handleForgotPassword(event) {
    event.preventDefault();
    const email = event.target.querySelector('input[type="email"]').value;
    
    showNotification(`Se han enviado las instrucciones de recuperación a ${email}`, 'success');
    closeForgotPassword();
    event.target.reset();
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    const icons = {
        success: '✅',
        error: '❌',
        info: 'ℹ️',
        warning: '⚠️'
    };
    
    const colors = {
        success: 'bg-green-500',
        error: 'bg-red-500',
        info: 'bg-blue-500',
        warning: 'bg-yellow-500'
    };
    
    notification.className = `flex items-center space-x-2 px-4 py-3 rounded-lg text-white shadow-lg fade-in ${colors[type]}`;
    notification.innerHTML = `
        <span>${icons[type]}</span>
        <span class="text-sm font-medium">${message}</span>
    `;
    
    document.getElementById('notifications').appendChild(notification);
    
    setTimeout(() => {
        notification.style.animation = 'fadeOut 0.3s ease-out forwards';
        setTimeout(() => {
            notification.remove();
        }, 300);
    }, 4000);
}

// Agregar animación de shake
const shakeKeyframes = `
    @@keyframes shake {
        0%, 100% { transform: translateX(0); }
        10%, 30%, 50%, 70%, 90% { transform: translateX(-5px); }
        20%, 40%, 60%, 80% { transform: translateX(5px); }
    }
    @@keyframes fadeOut {
        from { opacity: 1; transform: translateY(0); }
        to { opacity: 0; transform: translateY(-10px); }
    }
`;

const style = document.createElement('style');
style.textContent = shakeKeyframes;
document.head.appendChild(style);

// Inicializar
document.addEventListener('DOMContentLoaded', function() {
    // updateUserType(); // ELIMINADA

    // Auto-completar credenciales al hacer clic en los ejemplos
    document.addEventListener('click', function(event) {
        if (event.target.closest('.bg-gray-50')) {
            const emailInput = document.getElementById('email');
            const passwordInput = document.getElementById('password');
            // Por defecto cargar el primer usuario de ejemplo
            emailInput.value = 'carlos.mendoza@gmail.com';
            passwordInput.value = '123456';
            showNotification('Credenciales cargadas. ¡Haz clic en "Iniciar Sesión"!', 'info');
        }
    });
});

// Cerrar modal al hacer clic fuera
document.addEventListener('click', function(event) {
    if (event.target.id === 'forgotPasswordModal') {
        closeForgotPassword();
    }
});

// Atajos de teclado
document.addEventListener('keydown', function(event) {
    if (event.key === 'Escape') {
        closeForgotPassword();
    }
});
