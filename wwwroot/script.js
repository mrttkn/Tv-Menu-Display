const fetchMenuGroups = async () => {
    try {
        const response = await fetch('http://localhost:5023/api/Menu/grouped');
        if (!response.ok) {
            throw new Error('API hatası: ' + response.statusText);
        }
        const data = await response.json();

        // API'den gelen veriyi kontrol et
        if (!data || !Array.isArray(data)) {
            throw new Error('Geçersiz veri formatı');
        }

        return data; // Menü gruplarını döndür
    } catch (error) {
        console.error('API hatası:', error);
        return []; // Hata durumunda boş array döndür
    }
};

const displayMenuGroups = async () => {
    const menuGroups = await fetchMenuGroups();

    if (menuGroups.length === 0) {
        console.warn('Menü grupları bulunamadı.');
        return;
    }

    const categoryList = document.getElementById('category-list'); // Kategorileri göstereceğiniz elementin ID'si
    const selectedCategoryDisplay = document.getElementById('selected-category'); // Seçilen kategori display element

    menuGroups.forEach(group => {
        const categoryItem = document.createElement('div');
        categoryItem.classList.add('category-item');
        categoryItem.textContent = group.groupName; // groupName kullanarak grup adını ekle

        categoryItem.addEventListener('click', () => {
            selectedCategoryDisplay.textContent = group.groupName; // Seçilen kategori adı
        });

        categoryList.appendChild(categoryItem);
    });
};

// Sayfa yüklendiğinde menü gruplarını listele
document.addEventListener('DOMContentLoaded', displayMenuGroups);