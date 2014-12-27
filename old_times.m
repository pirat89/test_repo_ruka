% zpracovani otisku prstu

close all
%% predzpracovani obrazu
[I_orig,map] = imread('FingerPrint.bmp','bmp');
[height,width] = size(I_orig);
magnification = 190;    %vlastni zvetseni pro vsechny obrazy
im_tmp= ~im2bw(I_orig, map, 0.8); % negovane prahovani

% mini filtr pro odstraneni chyb typu 'ocko'
for i=2:height-1
    for j=2:width-1
        if im_tmp(i,j) == 0
            s = im_tmp(i,j-1)+im_tmp(i,j+1)+im_tmp(i-1,j)+im_tmp(i+1,j);
            if s ==4
                im_tmp(i,j) = 1;
            end
        end
    end
end
clear s; % nadale jiz nevyuzita promenna

im_tmp=bwmorph(im_tmp,'thin', inf); % ztenceni hran

% mini filtr pro upravu shluku pixelu v delte
for i=2:height-1
    for j=2:width-1
        if im_tmp(i,j) == 1
            % kontrola spodní trojice
            if im_tmp(i+1,j-1)+im_tmp(i+1,j)+im_tmp(i+1,j+1) == 3
                im_tmp(i+1,j) = 0;
            % kontrola prave trojice
            elseif im_tmp(i-1,j+1)+im_tmp(i,j+1)+im_tmp(i+1,j+1) == 3
                im_tmp(i,j+1) = 0;
            %kontrola leve trojice
            elseif im_tmp(i-1,j-1)+im_tmp(i,j-1)+im_tmp(i+1,j-1) == 3
                im_tmp(i,j-1) = 0;
            % kontrola horni trojice
            elseif im_tmp(i-1,j-1)+im_tmp(i-1,j)+im_tmp(i-1,j+1) == 3
                im_tmp(i-1,j) = 0;
            end
        end
    end
end

% copie predzpracovaneho obrazu
im_skelet = im_tmp;

figure();
imshow(I_orig, 'InitialMagnification', magnification)
title('Original otisk prstu');

figure();
imshow(im_tmp, 'InitialMagnification', magnification)
title('Skelet otisku prstu');

%% scanovani otisku z obrazu

% inicializace nekterych promennych

lines = 0; % pocet linii

% matice n*3, radek: [x y type]
% typy: 1 - zacatek/konec,2- zacatek/konec (blizko vidlice), 3 - vidlice
markants = [];

% pro kazdy radek obsahuje body vyskytujici se nejdale od stredu 
% (horizontalniho) (vlevo i vpravo). 
% hodnoty v matici jsou inicializovany na horizontalni stred
bound_matrix = zeros(height,2);

horizontal_middle = width/2;
for y=1:height
    bound_matrix(y,1) = horizontal_middle;
    bound_matrix(y,2) = horizontal_middle;
end
clear horizontal_middle;


% matice n*2, radek obsahuje souradnice pixelu, na kterych byl nalezen
% v sousedstvi obarveny pixel - nahrazuje zde frontu
tmp_pixs = [];

% flag pro poznamenani ze se mohou objevit v okoli dva obarvene pixely 
% pochazejici z jedne liniie
flag = 1; % pri nalezeni linie je pocitano s vyskytem dvou obarvenych pixelu

% flag urcijici, zda je zpracovavany pixel oznaceny jako konec linie
% skutecne jejim koncem, nebo je jeji konec v sousedstvi
not_end_line = 0;

pixels = 0; % pocet okolo zbarvenych pixelu

% pruchod obrazkem po radcich
for i=1:height
    for j=1:width
        
        
        if im_tmp(i,j) == 1
            flag = 1;
            lines = lines+1;
            %tmp_pixs = [j,i];
            x = j; y = i;   % inic souradnic pro pruchod hrany
            im_tmp(y,x) = 0;

           while (1)
               % nastaveni hrubych hranic otisku pro dany radek
               if x < bound_matrix(y,1)
                   bound_matrix(y,1) = x;
               elseif x > bound_matrix(y,2)
                   bound_matrix(y,2) = x;
               end

               % inicializace hranicnich souradnic pro pruchod okolnich pixelu
               if x > 1, from_x= x-1;
               else from_x = 1;
               end

               if y > 1, from_y= y-1;
               else from_y = 1;
               end

               if x < width, to_x= x+1;
               else to_x = width;
               end

               if y < height, to_y = y+1;
               else to_y = height;
               end
               
               % pro spravne urceni bodu vidlice
               origin_x = x;
               origin_y = y;
   
               % pruchod okoli
               for y=from_y:to_y
                   for x=from_x:to_x
                       if im_tmp(y,x) == 1 % v okoli byl nalezen obarveny pixel
                           im_tmp(y,x) = 0;
                           pixels = pixels+1;
                           if (pixels == 2 && flag == 0)
                               % vidlice
                               lines = lines+2;
                               markants = [markants; origin_x origin_y 3;
                                           origin_x origin_y 2;
                                           x y 2;
                                           tmp_pixs(1,:) 2];
                               % presun zaznamu pixelu na konec fronty(matice)
                               tmp_pixs = [tmp_pixs; tmp_pixs(1,:); x y];
                               tmp_pixs(1,:) = [];
                           elseif pixels == 3
                               lines = lines +1;
                               markants = [markants; x y 2];
                               tmp_pixs = [tmp_pixs; x y];
                               tmp_pixs(1,:) = [];
                           else
                               % pridat zaznam na zacatek fronty
                               tmp_pixs = [x y; tmp_pixs];
                           end
                       end
                   end
               end
               
               if flag == 1 && pixels == 1
                   % byl nalezen markant pri nahodnem nalezeni linie
                   markants = [markants; origin_x origin_y 1];
               end
               
               flag = 0;
               if pixels == 0
                   % potencionalni konec linie
                   % prohledat frontu zda v sousedstvi neni schovany pixel
                   % ktery je jiz jako markant oznaceny a ceka na pruchod
                   % pokud bude nalezen, byla tato linie falesne oznacena
                   % 2x a proto musi byt pocet linii dekrementovat
                   for y=origin_y-1:origin_y+1
                       for x=origin_x-1:origin_x+1
                           for z=1:size(tmp_pixs,1)
                               if (tmp_pixs(z,1) == x && tmp_pixs(z,2) == y)
                                   not_end_line = 1;
                                   lines = lines -1;
                                   tmp_pixs(z,:) = []; % neni treba zpracovavat
                                   break;
                               end
                           end
                       end
                   end
                   
                   if not_end_line == 0
                       % je to konec linie
                       markants = [markants; origin_x origin_y 1];
                   else
                       not_end_line = 0;
                   end
               end
               pixels = 0;
               if size(tmp_pixs,1) == 0 % nalezene linie projdeny
                   break;
               else
                   x = tmp_pixs(1,1);
                   y = tmp_pixs(1,2);
                   tmp_pixs(1,:) = [];
               end
           end
        end
    end
end

% uvolneni nadale nepotrebnych promennych
clear from_x from_y origin_x origin_y to_x to_y;
clear flag not_end_line pixels tmp_pixs;

%% odstraneni neplatnych markantu a urceni hran plochy otisku
% markanty, ktere se nachazi na aktualne oznacenych hranicich otisku a
% jejichz oznaceni nema souvislost s vidlici, jsou smazany jako falesne 
% markanty a jejich souradnice jsou ulozeny do pomocnych hranicnich matic.
% zaroven jsou smazany markanty ktere nemaji nic spolecneho s vidlici do
% vzdalenosti 1px od horni a spodni hrany obrazku
% po odstraneni markantu je provedena kontrola, zda nektery z odstranenych
% markantu nebyl nahodou validni (viz nize) a pripadne je navracen zpet

% inicializace dalsich promennych
left_edge = [];
right_edge =[];
i=1;
j=1;

% seradit markanty dle souradnic od shora dolu a zleva doprava
markants = sortrows(markants,[2,1]);

while i <= size(markants,1)
    
    if markants(i,3) ~= 1
        % tento markant je s urcitosti platny
        i = i+1;
        continue;
    elseif markants(i,2) < 3
        % odstranit markant nahore - mohlo by dojit k vetsimu zkresleni
        % plochy otisku kvuli zaobleni linie, linie ve spodni casti jsou
        % naopak spise rovne a je zadouci je odstranit pozdeji
        markants(i,:) = [];
        continue;
    end
    
    % souradnice
    x = markants(i,1);
    y = markants(i,2);
    
    if bound_matrix(y,1) == x   % markant na leve hranici
        markants(i,:)=[]; % odstraneni neplatneho markantu
        left_edge = [left_edge; x y];
        continue
    elseif bound_matrix(y,2) == x   % markant na prave hranici
        markants(i,:) = [];
        right_edge = [right_edge; x y];
        continue
    end
    
    if markants(i,2) > height-2
        % odstanit markant dole
        markants(i,:) = [];
        continue;
    end
    
    i = i+1;
end

% kontrola zda nebyl odstranen validni markant - hleda se "zub" v pomocnych
% maticich hranic vetsi nez 2px
i=2;
while i < size(left_edge,1) % leva strana
    if left_edge(i-1,1)+2 <left_edge(i,1) && left_edge(i+1,1)+2 <left_edge(i,1)
        % zub - tento markant je platny
        markants = [markants; left_edge(i,:) 1];
        left_edge(i,:) = [];
    end
    i=i+1;
end

i=2;
while i < size(right_edge,1) % prava strana
    if right_edge(i-1,1)-2 >right_edge(i,1) && right_edge(i+1,1)-2 >right_edge(i,1)
        % zub - tento markant je platny
        markants = [markants; right_edge(i,:) 1];
        right_edge(i,:) = [];
    end
    i=i+1;
end


%% Uprava hranic
% Upravou hranic je dosazeno vyssi presnosti vypoctene plochy otisku.
% Uprava je provadena tak, ze mezi dvema sousednimi falesnymi markanty se 
% hranice posunou v jedne polovine na hodnotu prvniho falesneho markantu
% a v druhe polovine na hodnotu druheho falesneho markantu. V souctu bude
% vysledek stejny, jako by byly oba falesne markanty spojeny useckou. 
 
% uprava leve hranice
for i=2:size(left_edge,1);

    % vertikalni stred mezi sousednimi markanty
    middle = round((left_edge(i,2) - left_edge(i-1,2))/2)+left_edge(i-1,2);
    
    for y=left_edge(i-1,2):left_edge(i,2)
        if y < middle 
            bound_matrix(y,1) = left_edge(i-1,1);
        else
            bound_matrix(y,1) = left_edge(i,1);
        end
    end
end

% uprava prave hranice
for i=2:size(right_edge,1);

    % vertikalni stred mezi sousednimi markanty
    middle = round((right_edge(i,2) - right_edge(i-1,2))/2)+right_edge(i-1,2);
    
    for y=right_edge(i-1,2):right_edge(i,2)
        if y < middle 
            bound_matrix(y,2) = right_edge(i-1,1);
        else
            bound_matrix(y,2) = right_edge(i,1);
        end
    end
end

for i=1:height
    for j=bound_matrix(i,1):bound_matrix(i,2)
        im_tmp(i,j) = 1;
    end
end

figure();
imshow(im_tmp, 'InitialMagnification', magnification)
title('Plocha zabrana otiskem prstu');

% uvolneni nadale nepotrebnych promennych
clear im_tmp y x i j middle left_edge right_edge;

%% vypocet plochy prstu
% jelikoz neni znamo dpi, je bran rozmer vyska = 1 palec = 25,4 mm

% vypocet rozlohy pixelu
pixel_size = (25.4/height)^2; % mm^2
pixels = 0;

% soucet pixelu plochy otisku prstu
for i=1:size(bound_matrix,1)
    pixels = pixels + bound_matrix(i,2) - bound_matrix(i,1)+1;
end

% vysledna plocha
fingerprint_size = pixels * pixel_size;
area_finger_perc = pixels/(height*width)*100;

% odstraneni nadale nepodstatnych promennych
clear width height z pixels bound_matrix;
%% vypis hodnot v Command WIndow a zakresleni markantu do obrazku

% okno s vyznacenymi markantyfigure();
figure();
imshow(im_skelet, 'InitialMagnification', magnification)
title('Otisk prstu s vyznacenymi markanty');
hold on

% serazeni markantu dle typu 
markants = sortrows(markants,3);

% vyznaceni markantu
for i=1:size(markants,1)
    if markants(i,3) < 3
        plot(markants(i,1),markants(i,2),'ro');
    else
        plot(markants(i,1),markants(i,2),'bo');
    end
end;

hold off;

disp(sprintf('Plocha otisku: %f mm^2',fingerprint_size));
disp(sprintf('Plocha zabrana otiskem v : %f %%',area_finger_perc));
disp(sprintf('Pocet linii: %d',lines));
disp(sprintf('Pocet nalezenych markantu celkem: %d \n',size(markants,1)));

% odstraneni zbylych nepodstatnych promennych
clear i magnification;

% zbyle promenne ponechany, daly by se vyuzit pro dalsi zpracovani