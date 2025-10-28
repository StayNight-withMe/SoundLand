import sys
import os
import asyncio
import aiohttp
from bs4 import BeautifulSoup
from fake_useragent import UserAgent
import re
from urllib.parse import urljoin
import time
import unicodedata

async def fetch_page(session, url, headers):
    try:
        async with session.get(url, headers=headers) as response:
            if response.status != 200:
                print(f"Ошибка: Статус ответа {response.status} для {url}", file=sys.stderr)
                return None
            return await response.text()
    except Exception as e:
        print(f"Ошибка при загрузке страницы {url}: {e}", file=sys.stderr)
        return None

async def download_file(session, url, filepath, headers):
    try:
        async with session.get(url, headers=headers) as response:
            if response.status == 200:
                os.makedirs(os.path.dirname(filepath), exist_ok=True)
                with open(filepath, 'wb') as f:
                    while True:
                        chunk = await response.content.read(8192)
                        if not chunk:
                            break
                        f.write(chunk)
            else:
                print(f"Ошибка: Не удалось скачать файл {url}, статус {response.status}", file=sys.stderr)
    except Exception as e:
        print(f"Ошибка при скачивании файла {url}: {e}", file=sys.stderr)

async def process_track(session, i, clean_titles, clean_authors, clean_durations, clean_imgURL, clean_downloadURL, headers, host, type_):
    try:
        if (type_ == 1 and not clean_downloadURL[i]) or (type_ == 2 and not clean_imgURL[i]):
            print(f"Ошибка: Отсутствует URL для трека {i} (type={type_})", file=sys.stderr)
            return None

        duration_safe = clean_durations[i].replace(':', '_')
        filename_base = f"{clean_titles[i]}_{clean_authors[i]}_{duration_safe}"
        file_path_song = os.path.join('temp_song', filename_base + '.mp3')
        file_path_img = os.path.join('temp_img', filename_base + '.jpg')

        if type_ == 1:
            await download_file(session, clean_downloadURL[i], file_path_song, headers)
        elif type_ == 2:
            await download_file(session, clean_imgURL[i], file_path_img, headers)

        return f"{filename_base}|{file_path_song}|{file_path_img}"
    except Exception as e:
        print(f"Ошибка при обработке трека {i}: {e}", file=sys.stderr)
        return None

async def main(search, type_, arg3, arg4):
    ua = UserAgent()
    agent = ua.random
    headers = {
        'user-agent': agent,
        'Cache-Control': 'no-cache',
        'Pragma': 'no-cache',
        'Accept': 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8'
    }

    host = 'https://eu.hitmo-top.com'
    # Если type == 1 и search состоит только из цифр, оборачиваем в кавычки
    if type_ == 1 and search.isdigit():
        search = f'"{search}"'
    search = search.replace(' ', '%20')
    search = f'({search})'
    link = f'https://eu.hitmo-top.com/search?q={search}'

    start_time = time.perf_counter()

    async with aiohttp.ClientSession() as session:
        page_text = await fetch_page(session, link, headers)
        if not page_text:
            return

        soup = BeautifulSoup(page_text, "html.parser")

        os.makedirs("temp_song", exist_ok=True)
        os.makedirs("temp_img", exist_ok=True)

        clean_titles = []
        clean_authors = []
        clean_durations = []
        clean_imgURL = []
        clean_downloadURL = []

        all_tittle = soup.find_all('div', class_='track__title')
        all_author = soup.find_all('div', class_='track__desc')
        all_durations = soup.find_all('div', class_='track__fulltime')
        all_imgURL = soup.find_all('div', class_='track__img')
        download_btn = soup.find_all('a', class_='track__download-btn')

        # Проверка согласованности данных
        lengths = [len(all_tittle), len(all_author), len(all_durations), len(all_imgURL), len(download_btn)]
        

        for title in all_tittle:
            clean_title = re.sub(r'[\\/*?:"<>|]', '', unicodedata.normalize('NFKC', title.text.strip()))
            clean_titles.append(clean_title)

        for author in all_author:
            clean_author = re.sub(r'[\\/*?:"<>|]', '', unicodedata.normalize('NFKC', author.text.strip()))
            clean_authors.append(clean_author)

        for duration in all_durations:
            clean_duration = duration.text.strip()
            clean_durations.append(clean_duration)

        for img in all_imgURL:
            img_style = img.get('style', '')
            img_url_match = re.search(r"url\('(.*?)'\)", img_style)
            clean_imgURL.append(img_url_match.group(1) if img_url_match else None)

        for btn in download_btn:
            url = btn.get('href')
            if url and not url.startswith('http'):
                url = urljoin(host, url)
            clean_downloadURL.append(url)

        # Отладочный вывод для проверки парсинга
        print(f"Запрос: {search}", file=sys.stderr)
        print(f"Найденные треки: {list(zip(clean_titles, clean_authors, clean_durations, clean_downloadURL))}", file=sys.stderr)

        if type_ == 1:
            image_name = arg3
            modified_image_name = unicodedata.normalize('NFKC', image_name)
            print(f"Ищем трек: {modified_image_name}", file=sys.stderr)
            found = False
            for i in range(len(clean_titles)):
                duration_safe = clean_durations[i].replace(':', '_')
                filename_base = f"{clean_titles[i]}_{clean_authors[i]}_{duration_safe}"
                print(f"Сравниваем: {filename_base} с {modified_image_name}", file=sys.stderr)
                if filename_base == modified_image_name:
                    if not clean_downloadURL[i]:
                        print(f"Ссылка на скачивание не найдена для трека: {filename_base}", file=sys.stderr)
                    else:
                        file_path_song = os.path.join('temp_song', filename_base + '.mp3')
                        file_path_img = os.path.join('temp_img', filename_base + '.jpg')
                        await download_file(session, clean_downloadURL[i], file_path_song, headers)
                        print(f"{filename_base}|{file_path_song}|{file_path_img}")
                    found = True
                    break
            if not found:
                print(f"Трек не найден для имени: {image_name}", file=sys.stderr)
            return  # Завершаем выполнение после type=1
        elif type_ == 2:
            if not arg3.isdigit() or not arg4.isdigit():
                print("Ошибка: start_index и end_index должны быть целыми числами для type=2", file=sys.stderr)
                return
            start_index = int(arg3)
            end_index = int(arg4)
            total_tracks = len(clean_titles)
            if start_index < 0 or start_index >= total_tracks:
                print(f"Недопустимый start_index: {start_index}, всего треков={total_tracks}", file=sys.stderr)
                return
            # Ограничиваем end_index количеством доступных треков
            end_index = min(end_index, total_tracks)
            tasks = [process_track(session, i, clean_titles, clean_authors, clean_durations, clean_imgURL, clean_downloadURL, headers, host, type_)
                     for i in range(start_index, end_index)]
            results = await asyncio.gather(*tasks)
            print("\n".join(filter(None, results)))
        else:
            print("Недопустимый тип", file=sys.stderr)
            return

    end_time = time.perf_counter()
    execution_time = end_time - start_time
    print(f"Время выполнения: {execution_time:.2f} секунд", file=sys.stderr)

if __name__ == "__main__":
    try:
        if len(sys.argv) != 5:
            print(f"Ожидалось 4 аргумента: search, type, arg3, arg4. Получено: {len(sys.argv) - 1}", file=sys.stderr)
            sys.exit(1)

        search = sys.argv[1]
        type_ = int(sys.argv[2])
        arg3 = sys.argv[3]
        arg4 = sys.argv[4]

        asyncio.run(main(search, type_, arg3, arg4))
    except ValueError as e:
        print(f"Ошибка: type должен быть целым числом. {e}", file=sys.stderr)
        sys.exit(1)
    except Exception as e:
        print(f"Ошибка: {e}", file=sys.stderr)
        sys.exit(1)