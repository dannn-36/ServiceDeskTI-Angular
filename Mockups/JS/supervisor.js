// Sample data for supervisor view
const supervisorData = {
    priorityTickets: [
        { id: 'TK-1250', title: 'Servidor principal caído', priority: 'urgent', agent: 'Ana López', time: '45 min' },
        { id: 'TK-1249', title: 'Base de datos lenta', priority: 'high', agent: 'Juan Pérez', time: '1.2h' },
        { id: 'TK-1248', title: 'Error en aplicación', priority: 'high', agent: 'Sofía Martínez', time: '2.1h' },
        { id: 'TK-1247', title: 'Problema de red', priority: 'medium', agent: 'Carlos Ruiz', time: '30 min' }
    ],
    teamMembers: [
        { name: 'Ana López', status: 'available', tickets: 6, avgTime: '1.8h', satisfaction: 4.9 },
        { name: 'Juan Pérez', status: 'busy', tickets: 8, avgTime: '2.1h', satisfaction: 4.7 },
        { name: 'Sofía Martínez', status: 'available', tickets: 5, avgTime: '1.5h', satisfaction: 4.8 },
        { name: 'Carlos Ruiz', status: 'available', tickets: 7, avgTime: '2.3h', satisfaction: 4.6 }
    ],
    tickets: [
        { 
            id: 'TK-1250', 
            title: 'Servidor principal caído', 
            user: 'Sistema Automático', 
            agent: 'Ana López', 
            status: 'en-progreso', 
            priority: 'urgent', 
            category: 'Infraestructura', 
            time: '45 min',
            supervisorIntervened: false,
            messages: [
                { type: 'user', author: 'Sistema Automático', time: '08:00', content: 'Alerta: Servidor principal caído. Requiere atención inmediata.' },
                { type: 'agent', author: 'Ana López', time: '08:15', content: 'Estoy revisando el servidor. Parece un problema de alimentación.' },
                { type: 'agent', author: 'Ana López', time: '08:30', content: 'He reiniciado el servidor. Monitoreando estabilidad.' }
            ]
        },
        { 
            id: 'TK-1249', 
            title: 'Base de datos responde lento', 
            user: 'María González', 
            agent: 'Juan Pérez', 
            status: 'abierto', 
            priority: 'high', 
            category: 'Base de Datos', 
            time: '1.2h',
            supervisorIntervened: false,
            messages: [
                { type: 'user', author: 'María González', time: '10:00', content: 'La base de datos está muy lenta. Las consultas tardan minutos.' },
                { type: 'agent', author: 'Juan Pérez', time: '10:20', content: 'Hola María, voy a revisar los índices de la base de datos.' },
                { type: 'user', author: 'María González', time: '10:25', content: 'Afecta a todo el equipo de contabilidad. Urgente.' }
            ]
        },
        { 
            id: 'TK-1248', 
            title: 'Error en módulo contable', 
            user: 'Roberto Silva', 
            agent: 'Sofía Martínez', 
            status: 'pendiente', 
            priority: 'medium', 
            category: 'Software', 
            time: '2.1h',
            supervisorIntervened: false,
            messages: [
                { type: 'user', author: 'Roberto Silva', time: '09:00', content: 'Error en el módulo contable: "No se puede guardar factura".' },
                { type: 'agent', author: 'Sofía Martínez', time: '09:15', content: 'Roberto, voy a verificar los permisos del módulo.' },
                { type: 'agent', author: 'Sofía Martínez', time: '09:45', content: 'He actualizado los permisos. ¿Puedes probar de nuevo?' },
                { type: 'user', author: 'Roberto Silva', time: '10:00', content: 'Ahora funciona. Gracias.' }
            ]
        }
    ],
    escalations: [
        { id: 'TK-1245', title: 'Falla crítica en producción', escalatedTo: 'Gerencia TI', reason: 'Impacto alto', time: '3h 15min', status: 'critical' },
        { id: 'TK-1243', title: 'Pérdida de datos cliente', escalatedTo: 'Director Técnico', reason: 'Datos sensibles', time: '5h 30min', status: 'critical' },
        { id: 'TK-1240', title: 'Caída de servicios web', escalatedTo: 'Equipo DevOps', reason: 'SLA vencido', time: '2h 45min', status: 'pending' }
    ],
    inactiveTickets: [
        { 
            id: 'TK-1200', 
            title: 'Problema de conexión resuelto', 
            user: 'Carlos Mendoza', 
            agent: 'Ana López', 
            status: 'resuelto', 
            priority: 'low', 
            category: 'Red', 
            time: '2 días', 
            closedDate: '2024-01-10',
            supervisorIntervened: false,
            messages: [
                { type: 'user', author: 'Carlos Mendoza', time: '08:00', content: 'Hola, no tengo internet en mi computadora.' },
                { type: 'agent', author: 'Ana López', time: '08:15', content: 'Hola Carlos, voy a revisar tu conexión. ¿Puedes reiniciar tu router?' },
                { type: 'user', author: 'Carlos Mendoza', time: '08:20', content: 'Ya lo hice, pero sigue sin funcionar.' },
                { type: 'agent', author: 'Ana López', time: '08:30', content: 'Parece un problema de configuración. He reseteado tu conexión. Prueba ahora.' },
                { type: 'user', author: 'Carlos Mendoza', time: '08:35', content: '¡Ya funciona! Gracias.' },
                { type: 'agent', author: 'Ana López', time: '08:40', content: 'Perfecto, me alegra haber ayudado. El ticket queda resuelto.' }
            ]
        },
        { 
            id: 'TK-1195', 
            title: 'Actualización de software completada', 
            user: 'María González', 
            agent: 'Juan Pérez', 
            status: 'resuelto', 
            priority: 'medium', 
            category: 'Software', 
            time: '1 semana', 
            closedDate: '2024-01-08',
            supervisorIntervened: false,
            messages: [
                { type: 'user', author: 'María González', time: '10:00', content: 'Excel se cierra solo cuando abro archivos.' },
                { type: 'agent', author: 'Juan Pérez', time: '10:20', content: 'Hola María, voy a reinstalar Office. Dame unos minutos.' },
                { type: 'user', author: 'María González', time: '10:25', content: 'Ok, espero.' },
                { type: 'agent', author: 'Juan Pérez', time: '11:00', content: 'Listo, Office reinstalado. Prueba abrir Excel ahora.' },
                { type: 'user', author: 'María González', time: '11:05', content: '¡Funciona perfecto! Gracias.' },
                { type: 'agent', author: 'Juan Pérez', time: '11:10', content: 'Excelente, ticket resuelto.' }
            ]
        },
        { 
            id: 'TK-1180', 
            title: 'Configuración de email finalizada', 
            user: 'Roberto Silva', 
            agent: 'Sofía Martínez', 
            status: 'resuelto', 
            priority: 'high', 
            category: 'Email', 
            time: '3 días', 
            closedDate: '2024-01-05',
            supervisorIntervened: false,
            messages: [
                { type: 'user', author: 'Roberto Silva', time: '09:00', content: 'No puedo enviar emails desde Outlook.' },
                { type: 'agent', author: 'Sofía Martínez', time: '09:15', content: 'Hola Roberto, voy a verificar la configuración de tu cuenta.' },
                { type: 'user', author: 'Roberto Silva', time: '09:20', content: 'Ok, gracias.' },
                { type: 'agent', author: 'Sofía Martínez', time: '09:45', content: 'He actualizado los servidores SMTP. Prueba enviar un email de prueba.' },
                { type: 'user', author: 'Roberto Silva', time: '09:50', content: '¡Ya se envía! Perfecto.' },
                { type: 'agent', author: 'Sofía Martínez', time: '09:55', content: 'Genial, ticket cerrado.' }
            ]
        }
    ]
};

let currentSection = 'dashboard';

function showSection(section, element) {
    // Hide all sections
    document.querySelectorAll('[id$="-section"]').forEach(el => el.classList.add('hidden'));
    
    // Show selected section
    document.getElementById(section + '-section').classList.remove('hidden');
    
    // Update sidebar
    document.querySelectorAll('.sidebar-item').forEach(item => item.classList.remove('active'));
    if (element) element.classList.add('active');

    currentSection = section;

    // Load section-specific data
    if (section === 'dashboard') loadDashboard();
    if (section === 'team') {
        const searchEl = document.getElementById('teamMemberSearch');
        if (searchEl && searchEl.value) filterTeamMembers(); else loadTeam();
    }
    if (section === 'tickets') {
        // Ejecuta el filtrado con los controles nuevos si existen
        if (document.getElementById('supervisorTicketSearch') ||
            document.getElementById('supervisorTicketStatusFilter') ||
            document.getElementById('supervisorTicketPriorityFilter') ||
            document.getElementById('supervisorTicketAgentFilter')) {
            filterSupervisorTickets();
        } else {
            loadTickets();
        }
    }
    if (section === 'inactive-tickets') loadInactiveTickets();
    if (section === 'workload') loadWorkload();
    if (section === 'performance') loadPerformance();
    if (section === 'escalations') loadEscalations();
}

function loadDashboard() {
    // Load priority tickets - removed, replaced with area chart
    // Load tickets by area chart in the right grid
    const areaCtx = document.getElementById('ticketsByAreaChart').getContext('2d');
    const areaChart = new Chart(areaCtx, {
        type: 'doughnut',
        data: {
            labels: ['💾 Software', '🌐 Red', '📧 Correo', '📞 Teléfono', '🔐 Accesos', '📝 Otro'],
            datasets: [{
                data: [0, 0, 0, 0, 0, 0], // Start with zero data
                backgroundColor: [
                    'rgba(59, 130, 246, 0.8)', // Blue for Software
                    'rgba(16, 185, 129, 0.8)', // Green for Red
                    'rgba(245, 158, 11, 0.8)', // Yellow for Correo
                    'rgba(139, 92, 246, 0.8)', // Purple for Teléfono
                    'rgba(239, 68, 68, 0.8)', // Red for Accesos
                    'rgba(107, 114, 128, 0.8)' // Gray for Otro
                ]
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 2000,
                easing: 'easeOutQuart'
            }
        }
    });
    // Animate to real data
    setTimeout(() => {
        areaChart.data.datasets[0].data = [25, 15, 10, 8, 12, 5];
        areaChart.update();
    }, 100);
}

// Cargar agentes (acepta lista filtrada)
function loadTeam(list = supervisorData.teamMembers) {
    const teamContainer = document.getElementById('teamMembers');
    teamContainer.innerHTML = list.map(member => `
        <div class="bg-gray-50 rounded-lg p-4">
            <div class="flex items-center justify-between mb-3">
                <div class="flex items-center space-x-3">
                    <div class="w-10 h-10 bg-blue-100 rounded-full flex items-center justify-center">
                        <span>👤</span>
                    </div>
                    <div>
                        <h4 class="font-medium text-gray-900">${member.name}</h4>
                        <p class="text-sm text-gray-600">Agente de Soporte</p>
                    </div>
                </div>
                <div class="flex items-center space-x-2">
                    <span class="w-3 h-3 rounded-full ${getStatusColor(member.status)}"></span>
                    <span class="text-sm text-gray-600 capitalize">${member.status}</span>
                </div>
            </div>
            <div class="grid grid-cols-3 gap-3 text-center">
                <div>
                    <p class="text-lg font-bold text-blue-600">${member.tickets}</p>
                    <p class="text-xs text-gray-600">Tickets</p>
                </div>
                <div>
                    <p class="text-lg font-bold text-green-600">${member.avgTime}</p>
                    <p class="text-xs text-gray-600">Promedio</p>
                </div>
                <div>
                    <p class="text-lg font-bold text-yellow-600">${member.satisfaction}</p>
                    <p class="text-xs text-gray-600">Rating</p>
                </div>
            </div>
            <div class="mt-3 flex space-x-2">
                <button class="flex-1 bg-blue-600 text-white py-1 px-2 rounded text-xs hover:bg-blue-700 transition-colors">
                    Asignar Ticket
                </button>
                <button class="flex-1 bg-gray-600 text-white py-1 px-2 rounded text-xs hover:bg-gray-700 transition-colors">
                    Ver Detalles
                </button>
            </div>
        </div>
    `).join('');
}

// Filtrado de agentes (insensible a acentos)
function filterTeamMembers() {
    const q = normalize(document.getElementById('teamMemberSearch')?.value || '');
    const filtered = supervisorData.teamMembers.filter(m => {
        const fields = [
            m.name, m.status, String(m.tickets), m.avgTime, String(m.satisfaction)
        ].map(normalize);
        return q === '' || fields.some(f => f.includes(q));
    });
    loadTeam(filtered);
}

function loadTickets(list = supervisorData.tickets) {
    const ticketsContainer = document.getElementById('supervisorTicketsList');
    ticketsContainer.innerHTML = list.map(ticket => `
        <div class="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50 priority-${ticket.priority}">
            <div class="flex items-center space-x-4">
                <span class="text-lg">${getCategoryIcon(ticket.category)}</span>
                <div>
                    <h4 class="font-medium text-gray-900">${ticket.title}</h4>
                    <p class="text-sm text-gray-600">${ticket.id} • ${ticket.user} → ${ticket.agent}</p>
                </div>
            </div>
            <div class="flex items-center space-x-3">
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getStatusBadgeClass(ticket.status)}">
                    ${ticket.status}
                </span>
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${getPriorityBadgeClass(ticket.priority)}">
                    ${ticket.priority}
                </span>
                <span class="text-sm text-gray-500">${ticket.time}</span>
                <button class="text-blue-600 hover:text-blue-800 text-sm font-medium"
                    onclick="openSuperviseModal('${ticket.id}')">
                    Supervisar
                </button>
            </div>
        </div>
    `).join('');
}

// Búsqueda y filtros de tickets (insensible a acentos/mayúsculas)
function filterSupervisorTickets() {
    const q = normalize(document.getElementById('supervisorTicketSearch')?.value || '');
    const status = (document.getElementById('supervisorTicketStatusFilter')?.value || '').toLowerCase();
    const priority = (document.getElementById('supervisorTicketPriorityFilter')?.value || '').toLowerCase();
    const agent = normalize(document.getElementById('supervisorTicketAgentFilter')?.value || '');

    const filtered = supervisorData.tickets.filter(t => {
        const matchesQuery = q === '' || [t.id, t.title, t.user, t.agent, t.status, t.priority, t.category, t.time]
            .map(normalize).some(f => f.includes(q));
        const matchesStatus = !status || (t.status || '').toLowerCase() === status;
        const matchesPriority = !priority || (t.priority || '').toLowerCase() === priority;
        const matchesAgent = !agent || normalize(t.agent) === agent;
        return matchesQuery && matchesStatus && matchesPriority && matchesAgent;
    });
    loadTickets(filtered);
}

// Normaliza texto (minúsculas y sin acentos)
function normalize(str = '') {
    return String(str).normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase();
}

// Filtra escalaciones por texto (id, título, destino, razón, estado, tiempo)
function filterEscalations() {
    const q = normalize(document.getElementById('escalationSearch')?.value || '');
    const filtered = supervisorData.escalations.filter(e => {
        const fields = [
            e.id, e.title, e.escalatedTo, e.reason, e.status, e.time,
            e.status === 'critical' ? 'critico' : 'pendiente' // búsqueda en español
        ].map(normalize);
        return q === '' || fields.some(f => f.includes(q));
    });
    loadEscalations(filtered);
}

// Cargar tickets inactivos
function loadInactiveTickets(list = supervisorData.inactiveTickets) {
    const container = document.getElementById('inactiveTicketsList');
    container.innerHTML = list.map(ticket => `
        <div class="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50">
            <div class="flex items-center space-x-4">
                <span class="text-lg">${getCategoryIcon(ticket.category)}</span>
                <div>
                    <h4 class="font-medium text-gray-900">${ticket.title}</h4>
                    <p class="text-sm text-gray-600">${ticket.id} • ${ticket.user} → ${ticket.agent}</p>
                    <p class="text-xs text-gray-500">Cerrado: ${ticket.closedDate}</p>
                </div>
            </div>
            <div class="flex items-center space-x-3">
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-green-100 text-green-800">
                    ${ticket.status}
                </span>
                <span class="text-sm text-gray-500">${ticket.time}</span>
                <button class="text-blue-600 hover:text-blue-800 text-sm font-medium"
                    onclick="openSuperviseInactiveModal('${ticket.id}')">
                    Supervisar
                </button>
            </div>
        </div>
    `).join('');
}

// Filtrar tickets inactivos
function filterInactiveTickets() {
    const q = normalize(document.getElementById('inactiveTicketSearch')?.value || '');
    const filtered = supervisorData.inactiveTickets.filter(t => {
        const fields = [t.id, t.title, t.user, t.agent, t.category, t.time].map(normalize);
        return q === '' || fields.some(f => f.includes(q));
    });
    loadInactiveTickets(filtered);
}

// Reabrir ticket
function reopenTicket(ticketId) {
    const ticket = supervisorData.inactiveTickets.find(t => t.id === ticketId);
    if (ticket) {
        ticket.status = 'abierto';
        supervisorData.tickets.push(ticket);
        supervisorData.inactiveTickets = supervisorData.inactiveTickets.filter(t => t.id !== ticketId);
        loadInactiveTickets();
        if (typeof filterSupervisorTickets === 'function') filterSupervisorTickets(); else loadTickets();
        showNotification(`Ticket ${ticketId} reabierto y movido a activos.`, 'success');
    }
}

function loadWorkload() {
    const workloadCtx = document.getElementById('workloadChart').getContext('2d');
    new Chart(workloadCtx, {
        type: 'bar',
        data: {
            labels: ['Ana López', 'Juan Pérez', 'Sofía Martínez', 'Carlos Ruiz'],
            datasets: [{
                label: 'Tickets Asignados',
                data: [6, 8, 5, 7],
                backgroundColor: 'rgba(59, 130, 246, 0.8)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                y: {
                    beginAtZero: true
                }
            }
        }
    });
}

function loadPerformance() {
    // Weekly Trend Chart
    const weeklyCtx = document.getElementById('weeklyTrendChart').getContext('2d');
    new Chart(weeklyCtx, {
        type: 'line',
        data: {
            labels: ['Lun', 'Mar', 'Mié', 'Jue', 'Vie', 'Sáb', 'Dom'],
            datasets: [{
                label: 'Tickets Resueltos',
                data: [12, 15, 18, 14, 16, 8, 5],
                borderColor: 'rgba(16, 185, 129, 1)',
                backgroundColor: 'rgba(16, 185, 129, 0.1)',
                tension: 0.4
            }, {
                label: 'Tickets Creados',
                data: [10, 16, 20, 15, 18, 10, 7],
                borderColor: 'rgba(59, 130, 246, 1)',
                backgroundColor: 'rgba(59, 130, 246, 0.1)',
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false
        }
    });

    // Agent Comparison Chart
    const agentCtx = document.getElementById('agentComparisonChart').getContext('2d');
    new Chart(agentCtx, {
        type: 'radar',
        data: {
            labels: ['Velocidad', 'Calidad', 'Satisfacción', 'Comunicación', 'Proactividad'],
            datasets: [{
                label: 'Ana López',
                data: [9, 8, 9, 8, 7],
                borderColor: 'rgba(59, 130, 246, 1)',
                backgroundColor: 'rgba(59, 130, 246, 0.2)'
            }, {
                label: 'Juan Pérez',
                data: [7, 9, 8, 9, 8],
                borderColor: 'rgba(16, 185, 129, 1)',
                backgroundColor: 'rgba(16, 185, 129, 0.2)'
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            scales: {
                r: {
                    beginAtZero: true,
                    max: 10
                }
            }
        }
    });
}

// Utility functions
function getPriorityIcon(priority) {
    const icons = { urgent: '🚨', high: '⚠️', medium: '📋', low: '📝' };
    return icons[priority] || '📋';
}

function getPriorityBadgeClass(priority) {
    const classes = {
        urgent: 'bg-red-100 text-red-800',
        high: 'bg-orange-100 text-orange-800',
        medium: 'bg-yellow-100 text-yellow-800',
        low: 'bg-green-100 text-green-800'
    };
    return classes[priority] || 'bg-gray-100 text-gray-800';
}

function getStatusBadgeClass(status) {
    const classes = {
        'abierto': 'bg-blue-100 text-blue-800',
        'en-progreso': 'bg-yellow-100 text-yellow-800',
        'pendiente': 'bg-purple-100 text-purple-800',
        'resuelto': 'bg-green-100 text-green-800'
    };
    return classes[status] || 'bg-gray-100 text-gray-800';
}

function getStatusColor(status) {
    const colors = {
        available: 'bg-green-500',
        busy: 'bg-yellow-500',
        away: 'bg-red-500'
    };
    return colors[status] || 'bg-gray-500';
}

function getCategoryIcon(category) {
    const icons = {
        'Infraestructura': '🏗️',
        'Base de Datos': '🗄️',
        'Software': '💾',
        'Hardware': '🖥️',
        'Red': '🌐'
    };
    return icons[category] || '❓';
}

function showNotifications() {
    showNotification('Panel de notificaciones en desarrollo', 'info');
}

function logout() {
    if (confirm('¿Estás seguro de que quieres cerrar sesión?')) {
        showNotification('Cerrando sesión...', 'info');
        setTimeout(() => {
            console.log('Redirect to login');
        }, 1500);
    }
}

// Add confirmLogout function
function confirmLogout() {
    const confirmExit = confirm("¿Deseas cerrar sesión?");
    if (confirmExit) {
        window.location.href = "/logout"; // cambia la ruta según tu backend
    }
}

function showNotification(message, type = 'info') {
    const notification = document.createElement('div');
    const icons = { success: '✅', error: '❌', info: 'ℹ️', warning: '⚠️' };
    const colors = { success: 'bg-green-500', error: 'bg-red-500', info: 'bg-blue-500', warning: 'bg-yellow-500' };
    
    notification.className = `fixed top-4 right-4 flex items-center space-x-2 px-4 py-3 rounded-lg text-white shadow-lg z-50 fade-in ${colors[type]}`;
    notification.innerHTML = `<span>${icons[type]}</span><span class="text-sm font-medium">${message}</span>`;
    
    document.body.appendChild(notification);
    
    setTimeout(() => {
        notification.remove();
    }, 3000);
}

// Variables para el modal de supervisión
let ticketToSupervise = null;

function openSuperviseModal(ticketId) {
    ticketToSupervise = supervisorData.tickets.find(t => t.id === ticketId);
    if (!ticketToSupervise) return;
    document.getElementById('superviseTicketTitle').textContent =
        `${ticketToSupervise.id} - ${ticketToSupervise.title}`;

    // Llenar select de agentes (solo los que no son el actual)
    const agentSelect = document.getElementById('superviseAgentSelect');
    agentSelect.innerHTML = supervisorData.teamMembers
        .filter(agent => agent.name !== ticketToSupervise.agent)
        .map(agent => `<option value="${agent.name}">${agent.name}</option>`)
        .join('');
    // Reset selects
    document.getElementById('superviseEscalateSelect').value = "";
    // Bloquear chat por defecto si no ha intervenido
    const input = document.getElementById('superviseChatInput');
    const btn = document.getElementById('superviseSendBtn');
    if (!ticketToSupervise.supervisorIntervened) {
        input.disabled = true;
        btn.disabled = true;
        input.placeholder = "Chat bloqueado - Haz clic en 'Intervenir' para escribir...";
    } else {
        input.disabled = false;
        btn.disabled = false;
        input.placeholder = "Escribe un mensaje...";
    }
    loadSuperviseChatMessages();
    document.getElementById('superviseModal').classList.remove('hidden');
}

function closeSuperviseModal() {
    document.getElementById('superviseModal').classList.add('hidden');
    ticketToSupervise = null;
}

// Reasignar desde modal de supervisión
function confirmReassignSupervise() {
    const select = document.getElementById('superviseAgentSelect');
    const newAgent = select.value;
    if (ticketToSupervise && newAgent) {
        ticketToSupervise.agent = newAgent;
        if (typeof filterSupervisorTickets === 'function') filterSupervisorTickets(); else loadTickets();
        showNotification('Ticket reasignado a ' + newAgent, 'success');
        closeSuperviseModal();
    }
}

// Escalar desde modal de supervisión
function confirmEscalateSupervise() {
    const level = document.getElementById('superviseEscalateSelect').value;
    if (ticketToSupervise && level) {
        ticketToSupervise.status = 'pendiente';
        supervisorData.escalations.push({
            id: ticketToSupervise.id,
            title: ticketToSupervise.title,
            escalatedTo: level,
            reason: 'Escalado manual por supervisor',
            time: 'Ahora',
            status: 'pending'
        });
        if (typeof filterSupervisorTickets === 'function') filterSupervisorTickets(); else loadTickets();
        if (typeof filterEscalations === 'function') filterEscalations(); else loadEscalations();
        showNotification(
            'Ticket escalado a la categoría: ' +
            document.getElementById('superviseEscalateSelect').options[
                document.getElementById('superviseEscalateSelect').selectedIndex
            ].text,
            'warning'
        );
        closeSuperviseModal();
    } else {
        showNotification('Selecciona una categoría para escalar.', 'error');
    }
}

// Resolver desde modal de supervisión
function confirmResolveSupervise() {
    if (ticketToSupervise) {
        ticketToSupervise.status = 'resuelto';
        if (typeof filterSupervisorTickets === 'function') filterSupervisorTickets(); else loadTickets();
        showNotification('Ticket resuelto por el supervisor', 'success');
        closeSuperviseModal();
    }
}

// Datos del supervisor (simulados)
let supervisorProfile = {
    name: 'Roberto Silva',
    email: 'roberto.silva@empresa.com'
};

// Mostrar modal de perfil
function showSupervisorProfileModal() {
    document.getElementById('supervisorInputName').value = supervisorProfile.name;
    document.getElementById('supervisorInputEmail').value = supervisorProfile.email;
    document.getElementById('supervisorProfileModal').classList.remove('hidden');
}
function closeSupervisorProfileModal() {
    document.getElementById('supervisorProfileModal').classList.add('hidden');
}
function updateSupervisorProfile(event) {
    event.preventDefault();
    supervisorProfile.name = document.getElementById('supervisorInputName').value;
    supervisorProfile.email = document.getElementById('supervisorInputEmail').value;
    document.getElementById('supervisorName').textContent = supervisorProfile.name;
    closeSupervisorProfileModal();
    showNotification('Perfil actualizado.', 'success');
}

function getSelectedAgentText() {
    const select = document.getElementById('reportAgentSelect');
    return select ? select.options[select.selectedIndex].text : 'Todos los agentes';
}

function generateReport(tipo) {
    let msg = '';
    if (tipo === 'semanal') {
        const format = document.getElementById('weeklyReportFormat').value;
        msg = `Generando reporte semanal en formato ${format === 'pdf' ? 'PDF' : 'Excel'}...`;
    } else if (tipo === 'individual') {
        const agent = document.getElementById('individualAgentSelect').value;
        const format = document.getElementById('individualReportFormat').value;
        msg = `Generando reporte de rendimiento individual para ${agent} en formato ${format === 'pdf' ? 'PDF' : 'Excel'}...`;
    } else if (tipo === 'sla') {
        const format = document.getElementById('slaReportFormat').value;
        msg = `Generando reporte SLA Compliance en formato ${format === 'pdf' ? 'PDF' : 'Excel'}...`;
    }
    showNotification(msg, 'info');
    setTimeout(() => {
        showNotification('Reporte descargado.', 'success');
    }, 1500);
}

// Render de escalaciones (restaurado)
function loadEscalations(list = supervisorData.escalations) {
    const escalationsContainer = document.getElementById('escalationsList');
    if (!escalationsContainer) return;
    escalationsContainer.innerHTML = list.map(escalation => `
        <div class="flex items-center justify-between p-4 border border-gray-200 rounded-lg ${escalation.status === 'critical' ? 'bg-red-50 border-red-200' : 'bg-yellow-50 border-yellow-200'}">
            <div class="flex items-center space-x-4">
                <span class="text-lg">${escalation.status === 'critical' ? '🚨' : '⚠️'}</span>
                <div>
                    <h4 class="font-medium text-gray-900">${escalation.title}</h4>
                    <p class="text-sm text-gray-600">${escalation.id} • Escalado a: ${escalation.escalatedTo}</p>
                    <p class="text-xs text-gray-500">Razón: ${escalation.reason}</p>
                </div>
            </div>
            <div class="text-right">
                <span class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium ${escalation.status === 'critical' ? 'bg-red-100 text-red-800' : 'bg-yellow-100 text-yellow-800'}">
                    ${escalation.status === 'critical' ? 'Crítico' : 'Pendiente'}
                </span>
                <p class="text-sm text-gray-500 mt-1">${escalation.time}</p>
            </div>
        </div>
    `).join('');
}

function loadSuperviseChatMessages() {
    const chatContainer = document.getElementById('superviseChatMessages');
    if (!ticketToSupervise || !ticketToSupervise.messages) return;
    
    chatContainer.innerHTML = ticketToSupervise.messages.map(message => `
        <div class="chat-message flex ${message.type === 'agent' || message.type === 'supervisor' ? 'justify-end' : 'justify-start'}">
            <div class="max-w-xs lg:max-w-md">
                <div class="flex items-center space-x-2 mb-1">
                    <span class="text-xs text-gray-500">${message.author}</span>
                    <span class="text-xs text-gray-400">${message.time}</span>
                </div>
                <div class="px-3 py-2 rounded-lg text-sm ${
                    message.type === 'agent' || message.type === 'supervisor' 
                        ? 'bg-blue-600 text-white' 
                        : 'bg-gray-100 text-gray-900'
                }">
                    ${message.content}
                </div>
            </div>
        </div>
    `).join('');
    chatContainer.scrollTop = chatContainer.scrollHeight;
}

function handleSuperviseChatKeyPress(event) {
    if (event.key === 'Enter') {
        sendSuperviseMessage();
    }
}

function sendSuperviseMessage() {
    const input = document.getElementById('superviseChatInput');
    const message = input.value.trim();
    
    if (!message || !ticketToSupervise) return;

    // Agregar mensaje del supervisor
    ticketToSupervise.messages.push({
        type: 'supervisor',
        author: 'Supervisor - ' + supervisorProfile.name,
        time: new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit' }),
        content: message
    });

    input.value = '';
    loadSuperviseChatMessages();
    showNotification('Mensaje enviado', 'success');
}

function interveneChat() {
    if (!ticketToSupervise) return;
    ticketToSupervise.supervisorIntervened = true;
    const input = document.getElementById('superviseChatInput');
    const btn = document.getElementById('superviseSendBtn');
    input.disabled = false;
    btn.disabled = false;
    input.placeholder = "Escribe un mensaje...";
    input.focus();
    showNotification('Intervención activada - Puedes escribir en el chat', 'info');
}

// Variables para el modal de supervisión inactiva
let ticketToSuperviseInactive = null;

function openSuperviseInactiveModal(ticketId) {
    ticketToSuperviseInactive = supervisorData.inactiveTickets.find(t => t.id === ticketId);
    if (!ticketToSuperviseInactive) return;
    document.getElementById('superviseInactiveTicketTitle').textContent =
        `${ticketToSuperviseInactive.id} - ${ticketToSuperviseInactive.title}`;

    // Always disable chat for inactive tickets (no intervention allowed)
    const input = document.getElementById('superviseInactiveChatInput');
    const btn = document.getElementById('superviseInactiveSendBtn');
    input.disabled = true;
    btn.disabled = true;
    input.placeholder = "Chat bloqueado - Solo lectura para tickets inactivos...";
    document.getElementById('reopenReason').value = '';
    loadSuperviseInactiveChatMessages();
    document.getElementById('superviseInactiveModal').classList.remove('hidden');
}

function closeSuperviseInactiveModal() {
    document.getElementById('superviseInactiveModal').classList.add('hidden');
    ticketToSuperviseInactive = null;
}

function loadSuperviseInactiveChatMessages() {
    const chatContainer = document.getElementById('superviseInactiveChatMessages');
    if (!ticketToSuperviseInactive || !ticketToSuperviseInactive.messages) return;
    
    chatContainer.innerHTML = ticketToSuperviseInactive.messages.map(message => `
        <div class="chat-message flex ${message.type === 'agent' || message.type === 'supervisor' ? 'justify-end' : 'justify-start'}">
            <div class="max-w-xs lg:max-w-md">
                <div class="flex items-center space-x-2 mb-1">
                    <span class="text-xs text-gray-500">${message.author}</span>
                    <span class="text-xs text-gray-400">${message.time}</span>
                </div>
                <div class="px-3 py-2 rounded-lg text-sm ${
                    message.type === 'agent' || message.type === 'supervisor' 
                        ? 'bg-blue-600 text-white' 
                        : 'bg-gray-100 text-gray-900'
                }">
                    ${message.content}
                </div>
            </div>
        </div>
    `).join('');
    chatContainer.scrollTop = chatContainer.scrollHeight;
}

function confirmReopenInactive() {
    const reason = document.getElementById('reopenReason').value.trim();
    if (!reason) {
        showNotification('Por favor, ingresa una razón para reabrir el ticket.', 'error');
        return;
    }
    if (ticketToSuperviseInactive) {
        ticketToSuperviseInactive.reopenReason = reason;
        ticketToSuperviseInactive.status = 'abierto';
        supervisorData.tickets.push(ticketToSuperviseInactive);
        supervisorData.inactiveTickets = supervisorData.inactiveTickets.filter(t => t.id !== ticketToSuperviseInactive.id);
        loadInactiveTickets();
        if (typeof filterSupervisorTickets === 'function') filterSupervisorTickets(); else loadTickets();
        showNotification(`Ticket ${ticketToSuperviseInactive.id} reabierto. Razón: ${reason}`, 'success');
        closeSuperviseInactiveModal();
    }
}

document.addEventListener('DOMContentLoaded', function() {
    loadDashboard();

    // Ticket Distribution Chart - Now Criticality Level Chart
    const distributionCtx = document.getElementById('ticketDistributionChart').getContext('2d');
    const distributionChart = new Chart(distributionCtx, {
        type: 'doughnut',
        data: {
            labels: ['Bajo', 'Medio', 'Alto', 'Urgente'],
            datasets: [{
                data: [0, 0, 0, 0], // Start with zero data
                backgroundColor: [
                    'rgba(34, 197, 94, 0.8)', // Green for Bajo
                    'rgba(245, 158, 11, 0.8)', // Yellow for Medio
                    'rgba(239, 68, 68, 0.8)', // Red for Alto
                    'rgba(147, 51, 234, 0.8)' // Purple for Urgente
                ]
            }]
        },
        options: {
            responsive: true,

            maintainAspectRatio: false,
            animation: {
                animateRotate: true,
                animateScale: true,
                duration: 2000,
                easing: 'easeOutQuart'
            }
        }
    });
    // Animate to real data
    setTimeout(() => {
        distributionChart.data.datasets[0].data = [10, 20, 15, 5];
        distributionChart.update();
    }, 100);

    // Activar búsqueda en "Escalaciones Activas"
    const escSearch = document.getElementById('escalationSearch');
    if (escSearch) escSearch.addEventListener('input', filterEscalations);

   

    // Activar búsqueda en "Tickets Activos"
    const ticketsSearch = document.getElementById('supervisorTicketSearch');
    const statusSel = document.getElementById('supervisorTicketStatusFilter');
    const prioritySel = document.getElementById('supervisorTicketPriorityFilter');
    const agentSel = document.getElementById('supervisorTicketAgentFilter');
    if (ticketsSearch) ticketsSearch.addEventListener('input', filterSupervisorTickets);
    if (statusSel) statusSel.addEventListener('change', filterSupervisorTickets);
    if (prioritySel) prioritySel.addEventListener('change', filterSupervisorTickets);
    if (agentSel) agentSel.addEventListener('change', filterSupervisorTickets);

    // Activar búsqueda en "Agentes del Equipo"
    const teamSearch = document.getElementById('teamMemberSearch');
    if (teamSearch) teamSearch.addEventListener('input', filterTeamMembers);

    // Activar búsqueda en "Tickets Inactivos"
    const inactiveSearch = document.getElementById('inactiveTicketSearch');
    if (inactiveSearch) inactiveSearch.addEventListener('input', filterInactiveTickets);
});
