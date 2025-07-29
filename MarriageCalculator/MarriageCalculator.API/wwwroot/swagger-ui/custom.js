// Custom JavaScript for Enhanced Swagger UI - Marriage Calculator API

window.addEventListener('DOMContentLoaded', function() {
    // Add custom functionality once Swagger UI loads
    setTimeout(function() {
        enhanceSwaggerUI();
    }, 1000);
});

function enhanceSwaggerUI() {
    // Add keyboard shortcuts
    addKeyboardShortcuts();
    
    // Add quick navigation
    addQuickNavigation();
    
    // Add response time tracking
    addResponseTimeTracking();
    
    // Add copy to clipboard functionality
    addCopyToClipboard();
    
    // Add endpoint statistics
    addEndpointStatistics();
}

function addKeyboardShortcuts() {
    document.addEventListener('keydown', function(e) {
        // Ctrl+/ or Cmd+/ to focus search
        if ((e.ctrlKey || e.metaKey) && e.key === '/') {
            e.preventDefault();
            const filterInput = document.querySelector('.filter input');
            if (filterInput) {
                filterInput.focus();
            }
        }
        
        // Escape to collapse all sections
        if (e.key === 'Escape') {
            const openSections = document.querySelectorAll('.opblock.is-open');
            openSections.forEach(section => {
                const button = section.querySelector('.opblock-summary');
                if (button) button.click();
            });
        }
    });
}

function addQuickNavigation() {
    // Add a floating quick navigation panel
    const nav = document.createElement('div');
    nav.id = 'quick-nav';
    nav.innerHTML = `
        <div style="position: fixed; top: 50%; right: 20px; transform: translateY(-50%); 
                    background: white; border-radius: 8px; box-shadow: 0 4px 20px rgba(0,0,0,0.1); 
                    padding: 15px; z-index: 1000; max-height: 400px; overflow-y: auto; min-width: 200px;">
            <h4 style="margin: 0 0 10px 0; color: #2c3e50; font-size: 14px;">Quick Navigation</h4>
            <div id="nav-links"></div>
        </div>
    `;
    
    document.body.appendChild(nav);
    
    // Populate navigation links
    setTimeout(() => {
        const sections = document.querySelectorAll('.opblock-tag');
        const navLinks = document.getElementById('nav-links');
        
        sections.forEach((section, index) => {
            const link = document.createElement('a');
            link.href = '#';
            link.textContent = section.textContent.trim();
            link.style.cssText = 'display: block; padding: 5px 0; color: #3498db; text-decoration: none; font-size: 12px;';
            link.addEventListener('click', (e) => {
                e.preventDefault();
                section.scrollIntoView({ behavior: 'smooth' });
            });
            navLinks.appendChild(link);
        });
    }, 2000);
}

function addResponseTimeTracking() {
    // Track and display response times for API calls
    const originalFetch = window.fetch;
    
    window.fetch = function(...args) {
        const startTime = performance.now();
        
        return originalFetch.apply(this, args)
            .then(response => {
                const endTime = performance.now();
                const duration = Math.round(endTime - startTime);
                
                console.log(`API Response Time: ${duration}ms for ${args[0]}`);
                
                // Add response time to UI if possible
                setTimeout(() => {
                    const responseHeaders = document.querySelector('.responses .response .response-col_description');
                    if (responseHeaders) {
                        const timeElement = document.createElement('div');
                        timeElement.style.cssText = 'color: #27ae60; font-size: 12px; margin-top: 5px;';
                        timeElement.textContent = `? Response Time: ${duration}ms`;
                        responseHeaders.appendChild(timeElement);
                    }
                }, 100);
                
                return response;
            });
    };
}

function addCopyToClipboard() {
    // Add copy buttons for code examples
    setTimeout(() => {
        const codeBlocks = document.querySelectorAll('pre code');
        codeBlocks.forEach(block => {
            const button = document.createElement('button');
            button.textContent = '?? Copy';
            button.style.cssText = `
                position: absolute; top: 5px; right: 5px; 
                background: #3498db; color: white; border: none; 
                border-radius: 4px; padding: 4px 8px; font-size: 11px; 
                cursor: pointer; z-index: 10;
            `;
            
            button.addEventListener('click', () => {
                navigator.clipboard.writeText(block.textContent)
                    .then(() => {
                        button.textContent = '? Copied!';
                        setTimeout(() => {
                            button.textContent = '?? Copy';
                        }, 2000);
                    });
            });
            
            block.parentElement.style.position = 'relative';
            block.parentElement.appendChild(button);
        });
    }, 3000);
}

function addEndpointStatistics() {
    // Add endpoint statistics to the header
    setTimeout(() => {
        const operations = document.querySelectorAll('.opblock');
        const methodCounts = {};
        
        operations.forEach(op => {
            const method = op.className.match(/opblock-(\w+)/)?.[1];
            if (method) {
                methodCounts[method] = (methodCounts[method] || 0) + 1;
            }
        });
        
        const statsContainer = document.createElement('div');
        statsContainer.style.cssText = `
            background: #ecf0f1; padding: 15px; margin: 20px 0; 
            border-radius: 8px; text-align: center;
        `;
        
        const statsHTML = Object.entries(methodCounts)
            .map(([method, count]) => `
                <span style="display: inline-block; margin: 0 10px; padding: 5px 10px; 
                           background: white; border-radius: 4px; font-size: 12px;">
                    <strong>${method.toUpperCase()}</strong>: ${count}
                </span>
            `).join('');
        
        statsContainer.innerHTML = `
            <h4 style="margin: 0 0 10px 0; color: #2c3e50;">API Statistics</h4>
            ${statsHTML}
            <div style="margin-top: 10px; font-size: 11px; color: #7f8c8d;">
                Total Endpoints: ${operations.length}
            </div>
        `;
        
        const infoSection = document.querySelector('.info');
        if (infoSection) {
            infoSection.appendChild(statsContainer);
        }
    }, 2000);
}

// Add helpful tips
console.log(`
?? Marriage Calculator API - Enhanced Swagger UI Loaded!

Keyboard Shortcuts:
- Ctrl+/ (Cmd+/) : Focus search
- Escape : Collapse all sections

Features:
- ? Response time tracking
- ?? Copy to clipboard for code
- ?? Quick navigation panel
- ?? Endpoint statistics

Happy API testing! ??
`);