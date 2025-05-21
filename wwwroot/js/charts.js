// charts.js - Functions for creating charts using Chart.js

// Function to create a pie chart for task status distribution
function createTaskStatusChart(canvasId, data) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    
    const statusColors = {
        'Not Started': '#6c757d', // secondary
        'In Progress': '#0d6efd', // primary
        'Completed': '#198754',   // success
        'On Hold': '#ffc107'      // warning
    };
    
    const backgroundColor = data.map(item => statusColors[item.status]);
    
    new Chart(ctx, {
        type: 'pie',
        data: {
            labels: data.map(item => item.status),
            datasets: [{
                data: data.map(item => item.count),
                backgroundColor: backgroundColor,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'bottom',
                },
                title: {
                    display: true,
                    text: 'Tasks by Status'
                }
            }
        }
    });
}

// Function to create a bar chart for task priority distribution
function createTaskPriorityChart(canvasId, data) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    
    const priorityColors = {
        'Low': '#0dcaf0',    // info
        'Medium': '#fd7e14',  // orange
        'High': '#dc3545'     // danger
    };
    
    const backgroundColor = Object.keys(data).map(key => priorityColors[key]);
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: Object.keys(data),
            datasets: [{
                label: 'Tasks by Priority',
                data: Object.values(data),
                backgroundColor: backgroundColor,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            },
            plugins: {
                title: {
                    display: true,
                    text: 'Tasks by Priority'
                }
            }
        }
    });
}

// Function to create a horizontal bar chart for tasks per user
function createTasksPerUserChart(canvasId, data) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    console.log(data);
    // Sort data by count in descending order
    const sortedData = Object.entries(data)
        .sort((a, b) => b[1] - a[1])
        .slice(0, 10); // Show only top 10 users
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: sortedData.map(item => item[0]),
            datasets: [{
                label: 'Tasks Assigned',
                data: sortedData.map(item => item[1]),
                backgroundColor: '#0d6efd',
                borderWidth: 1
            }]
        },
        options: {
            indexAxis: 'y',
            responsive: true,
            scales: {
                x: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            },
            plugins: {
                title: {
                    display: true,
                    text: 'Tasks per User'
                }
            }
        }
    });
}

// Function to create a line chart for weekly completion trend
function createWeeklyCompletionChart(canvasId, data) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    
    new Chart(ctx, {
        type: 'line',
        data: {
            labels: data.map(item => item.week),
            datasets: [{
                label: 'Completed Tasks',
                data: data.map(item => item.count),
                borderColor: '#198754',
                backgroundColor: 'rgba(25, 135, 84, 0.1)',
                borderWidth: 2,
                fill: true,
                tension: 0.4
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            },
            plugins: {
                title: {
                    display: true,
                    text: 'Weekly Task Completion Trend'
                }
            }
        }
    });
}

// Function to create a bar chart for monthly completion trend
function createMonthlyCompletionChart(canvasId, data) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    
    new Chart(ctx, {
        type: 'bar',
        data: {
            labels: data.map(item => item.Month),
            datasets: [{
                label: 'Completed Tasks',
                data: data.map(item => item.Count),
                backgroundColor: '#198754',
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            scales: {
                y: {
                    beginAtZero: true,
                    ticks: {
                        precision: 0
                    }
                }
            },
            plugins: {
                title: {
                    display: true,
                    text: 'Monthly Task Completion Trend'
                }
            }
        }
    });
}

// Function to create a doughnut chart for tasks by project
function createTasksByProjectChart(canvasId, data) {
    const ctx = document.getElementById(canvasId).getContext('2d');
    
    // Generate random colors for each project
    const generateColors = (count) => {
        const colors = [];
        for (let i = 0; i < count; i++) {
            const hue = (i * 137) % 360; // Use golden angle to get evenly distributed colors
            colors.push(`hsl(${hue}, 70%, 60%)`);
        }
        return colors;
    };
    
    const projects = Object.keys(data);
    const counts = Object.values(data);
    const backgroundColor = generateColors(projects.length);
    
    new Chart(ctx, {
        type: 'doughnut',
        data: {
            labels: projects,
            datasets: [{
                data: counts,
                backgroundColor: backgroundColor,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true,
            plugins: {
                legend: {
                    position: 'bottom',
                },
                title: {
                    display: true,
                    text: 'Tasks by Project'
                }
            }
        }
    });
} 